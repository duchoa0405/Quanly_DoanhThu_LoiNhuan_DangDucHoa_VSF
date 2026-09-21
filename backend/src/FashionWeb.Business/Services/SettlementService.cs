using FashionWeb.Business.Commands;
using FashionWeb.Business.Domain.Entities;
using FashionWeb.Business.Domain.Enums;
using FashionWeb.Business.Exceptions;
using FashionWeb.Business.Filters;
using FashionWeb.Business.Interfaces.Repositories;
using FashionWeb.Business.Interfaces.Services;
using FashionWeb.Business.Results;

namespace FashionWeb.Business.Services;

public class SettlementService : ISettlementService
{
    private readonly IReconciliationRepository _reconciliationRepository;
    private readonly IDiscrepancyRepository _discrepancyRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TimeProvider _timeProvider;

    public SettlementService(
        IReconciliationRepository reconciliationRepository,
        IDiscrepancyRepository discrepancyRepository,
        IOrderRepository orderRepository,
        IUnitOfWork unitOfWork,
        TimeProvider? timeProvider = null)
    {
        _reconciliationRepository = reconciliationRepository;
        _discrepancyRepository = discrepancyRepository;
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    public async Task<PagedResult<SettlementLedgerResult>> GetLedgerAsync(SettlementQueryFilter filter, CancellationToken ct = default)
    {
        return await _reconciliationRepository.ListLedgerAsync(filter, ct);
    }

    public async Task<SettlementSummaryResult> GetSummaryAsync(DateTime? fromDate = null, DateTime? toDate = null, ChannelType? channel = null, CancellationToken ct = default)
    {
        return await _reconciliationRepository.GetSummaryAsync(fromDate, toDate, channel, ct);
    }

    public async Task<ReconciliationRecord> ReconcileAsync(ReconcileSettlementCommand command, CancellationToken ct = default)
    {
        if (command.ActualSettlement < 0m)
            throw new ValidationException("Actual settlement amount cannot be negative.");

        var order = await _orderRepository.GetOrderDetailByIdAsync(command.OrderId, ct);
        if (order == null)
            throw new NotFoundException($"Order with ID '{command.OrderId}' was not found.");

        if (order.Status != OrderStatus.DELIVERED)
            throw new BusinessRuleException($"Cannot reconcile settlement for order '{command.OrderId}' in status '{order.Status}'. Order must be DELIVERED.");

        var record = await _reconciliationRepository.GetByOrderIdAsync(command.OrderId, ct);
        if (record == null)
            throw new BusinessRuleException($"Reconciliation record for order '{command.OrderId}' was not found.");

        if (record.Status == ReconciliationStatus.RECONCILED)
            throw new BusinessRuleException($"Order '{command.OrderId}' has already been marked as RECONCILED.");

        DiscrepancyAudit? audit = null;
        var now = _timeProvider.GetUtcNow().UtcDateTime;

        await _unitOfWork.ExecuteTransactionAsync(async () =>
        {
            audit = record.Reconcile(command.ActualSettlement, command.Notes, command.DiscrepancyType, command.ActorIdentity, now);

            await _reconciliationRepository.UpdateAsync(record, ct);

            if (audit != null)
            {
                await _discrepancyRepository.AddAsync(audit, ct);
            }
        }, ct);

        return record;
    }
}
