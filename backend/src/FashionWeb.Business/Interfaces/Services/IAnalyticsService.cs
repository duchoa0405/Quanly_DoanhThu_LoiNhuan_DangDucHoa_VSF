namespace FashionWeb.Business.Interfaces.Services;

public interface IAnalyticsService
{
    Task<object> GetKpisAsync(DateTime? fromDate, DateTime? toDate);
    Task<object> GetDailyCashflowTrendAsync(int days);
    Task<object> GetChannelShareAsync();
    Task<object> GetTopSkusAsync(int limit);
    Task<byte[]> ExportReconciliationCsvAsync();
}
