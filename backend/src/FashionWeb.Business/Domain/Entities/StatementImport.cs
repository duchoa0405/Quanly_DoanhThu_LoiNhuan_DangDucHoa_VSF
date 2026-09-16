using FashionWeb.Business.Domain.Enums;

namespace FashionWeb.Business.Domain.Entities;

public class StatementImport
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public ChannelType Channel { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FileSha256 { get; set; } = string.Empty;
    public int TotalRows { get; set; }
    public int MatchedRows { get; set; }
    public int DiscrepancyRows { get; set; }
    public decimal TotalWalletSettled { get; set; }
    public DateTime ImportedAt { get; set; } = DateTime.UtcNow;
    public string ImportedBy { get; set; } = "System";
}
