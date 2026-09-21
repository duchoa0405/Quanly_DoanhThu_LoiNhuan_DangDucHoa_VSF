namespace FashionWeb.Business.Commands;

public record ResolveDiscrepancyCommand(
    Guid DiscrepancyId,
    string ResolutionNotes,
    string ActorIdentity
);
