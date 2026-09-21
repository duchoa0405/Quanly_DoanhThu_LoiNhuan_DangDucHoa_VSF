using FashionWeb.Business.Commands;
using FashionWeb.Business.Domain.Entities;
using FashionWeb.Business.Domain.Enums;
using FashionWeb.Business.Filters;
using FashionWeb.Business.Results;

namespace FashionWeb.Business.Interfaces.Services;

public interface ISettlementService
{
    Task<PagedResult<SettlementLedgerResult>> GetLedgerAsync(SettlementQueryFilter filter, CancellationToken ct = default);
    Task<SettlementSummaryResult> GetSummaryAsync(DateTime? fromDate = null, DateTime? toDate = null, ChannelType? channel = null, CancellationToken ct = default);
    Task<ReconciliationRecord> ReconcileAsync(ReconcileSettlementCommand command, CancellationToken ct = default);
}
