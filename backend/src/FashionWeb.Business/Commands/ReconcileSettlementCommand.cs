using FashionWeb.Business.Domain.Enums;

namespace FashionWeb.Business.Commands;

public record ReconcileSettlementCommand(
    Guid OrderId,
    decimal ActualSettlement,
    string? Notes,
    DiscrepancyType? DiscrepancyType,
    string ActorIdentity
);
