using FashionWeb.Business.Domain.Entities;
using FashionWeb.Business.Domain.Enums;
using FashionWeb.Business.Filters;
using FashionWeb.Business.Results;

namespace FashionWeb.Business.Interfaces.Repositories;

public interface IReconciliationRepository
{
    Task<ReconciliationRecord?> GetByOrderIdAsync(Guid orderId, CancellationToken ct = default);
    Task<ReconciliationRecord?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<PagedResult<SettlementLedgerResult>> ListLedgerAsync(SettlementQueryFilter filter, CancellationToken ct = default);
    Task<SettlementSummaryResult> GetSummaryAsync(DateTime? fromDate, DateTime? toDate, ChannelType? channel, CancellationToken ct = default);
    Task AddAsync(ReconciliationRecord record, CancellationToken ct = default);
    Task UpdateAsync(ReconciliationRecord record, CancellationToken ct = default);
}
