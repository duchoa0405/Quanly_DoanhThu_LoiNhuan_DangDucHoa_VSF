using FashionWeb.Business.Commands;
using FashionWeb.Business.Domain.Entities;
using FashionWeb.Business.Domain.Enums;
using FashionWeb.Business.Filters;
using FashionWeb.Business.Interfaces.Repositories;
using FashionWeb.Business.Interfaces.Services;
using FashionWeb.Business.Results;

namespace FashionWeb.Business.Services;

public class SettlementService : ISettlementService
{
    private readonly IReconciliationRepository _reconRepo;
    private readonly IDiscrepancyRepository _discrepancyRepo;
    private readonly IOrderRepository _orderRepo;
    private readonly IUnitOfWork _unitOfWork;

    public SettlementService(
        IReconciliationRepository reconRepo,
        IDiscrepancyRepository discrepancyRepo,
        IOrderRepository orderRepo,
        IUnitOfWork unitOfWork)
    {
        _reconRepo = reconRepo;
        _discrepancyRepo = discrepancyRepo;
        _orderRepo = orderRepo;
        _unitOfWork = unitOfWork;
    }

    public async Task<PagedResult<SettlementLedgerResult>> GetLedgerAsync(SettlementQueryFilter filter, CancellationToken ct = default)
    {
        return await _reconRepo.ListLedgerAsync(filter, ct);
    }

    public async Task<SettlementSummaryResult> GetSummaryAsync(DateTime? fromDate = null, DateTime? toDate = null, ChannelType? channel = null, CancellationToken ct = default)
    {
        return await _reconRepo.GetSummaryAsync(fromDate, toDate, channel, ct);
    }

    public async Task<ReconciliationRecord> ReconcileAsync(ReconcileSettlementCommand command, CancellationToken ct = default)
    {
        if (command.ActualSettlement < 0m)
            throw new ArgumentException("Actual settlement amount cannot be negative.", nameof(command));

        var order = await _orderRepo.GetOrderDetailByIdAsync(command.OrderId, ct);
        if (order == null)
            throw new KeyNotFoundException($"Order with ID '{command.OrderId}' was not found.");

        if (order.Status != OrderStatus.DELIVERED)
            throw new InvalidOperationException($"Cannot reconcile settlement for order '{command.OrderId}' in status '{order.Status}'. Order must be DELIVERED.");

        var record = await _reconRepo.GetByOrderIdAsync(command.OrderId, ct);
        if (record == null)
            throw new InvalidOperationException($"Reconciliation record for order '{command.OrderId}' was not found.");

        if (record.Status == ReconciliationStatus.RECONCILED)
            throw new InvalidOperationException($"Order '{command.OrderId}' has already been marked as RECONCILED.");

        DiscrepancyAudit? audit = null;

        await _unitOfWork.ExecuteTransactionAsync(async () =>
        {
            audit = record.Reconcile(command.ActualSettlement, command.Notes, command.DiscrepancyType, command.ActorIdentity);

            await _reconRepo.UpdateAsync(record, ct);

            if (audit != null)
            {
                await _discrepancyRepo.AddAsync(audit, ct);
            }
        }, ct);

        return record;
    }
}
