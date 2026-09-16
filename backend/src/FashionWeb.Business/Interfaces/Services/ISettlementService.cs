namespace FashionWeb.Business.Interfaces.Services;

public interface ISettlementService
{
    Task<object> GetLedgerAsync(string? channel, string? status, int page, int pageSize);
    Task<object> ImportStatementAsync(Stream fileStream, string fileName, string channelCode);
}
