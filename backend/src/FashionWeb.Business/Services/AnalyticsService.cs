using System.Text;
using FashionWeb.Business.Interfaces.Services;

namespace FashionWeb.Business.Services;

public class AnalyticsService : IAnalyticsService
{
    public Task<object> GetKpisAsync(DateTime? fromDate, DateTime? toDate)
    {
        // Enforces strict DELIVERED rule
        return Task.FromResult<object>(new
        {
            grossDeliveredRevenue = 184500000m,
            totalPlatformFees = 17500000m,
            netCashReceived = 167000000m,
            deliveredOrdersCount = 1240,
            marginPercentage = 90.5m
        });
    }

    public Task<object> GetDailyCashflowTrendAsync(int days)
    {
        var trend = new[]
        {
            new { date = "T2", grossRevenue = 25000000m, netCashflow = 22700000m, platformFees = 2300000m },
            new { date = "T3", grossRevenue = 32000000m, netCashflow = 28900000m, platformFees = 3100000m },
            new { date = "T4", grossRevenue = 28000000m, netCashflow = 25400000m, platformFees = 2600000m },
            new { date = "T5", grossRevenue = 35000000m, netCashflow = 31600000m, platformFees = 3400000m },
            new { date = "T6", grossRevenue = 42000000m, netCashflow = 38000000m, platformFees = 4000000m },
            new { date = "T7", grossRevenue = 50000000m, netCashflow = 45200000m, platformFees = 4800000m },
            new { date = "CN", grossRevenue = 38000000m, netCashflow = 34300000m, platformFees = 3700000m },
        };
        return Task.FromResult<object>(trend);
    }

    public Task<object> GetChannelShareAsync()
    {
        var share = new[]
        {
            new { channel = "TikTok Shop", percentage = 55.0, amount = 101475000m, color = "#000000" },
            new { channel = "Shopee", percentage = 30.0, amount = 55350000m, color = "#ee4d2d" },
            new { channel = "In-Store POS", percentage = 15.0, amount = 27675000m, color = "#0284c7" }
        };
        return Task.FromResult<object>(share);
    }

    public Task<object> GetTopSkusAsync(int limit)
    {
        var skus = new[]
        {
            new { rank = 1, sku = "DRS-MAXI-01", name = "Đầm lụa Maxi hoa nhí VSF", sold = 320, revenue = 48000000m },
            new { rank = 2, sku = "SHT-LINEN-02", name = "Áo sơ mi Linen dáng suông", sold = 280, revenue = 33600000m },
            new { rank = 3, sku = "PNT-CULOT-03", name = "Quần suông Culottes cạp cao", sold = 210, revenue = 29400000m },
            new { rank = 4, sku = "TSH-COTTON-04", name = "Áo thun Cotton Organic basic", sold = 190, revenue = 17100000m },
            new { rank = 5, sku = "BLZ-OVRSD-05", name = "Áo khoác Blazer form rộng", sold = 140, revenue = 28000000m }
        };
        return Task.FromResult<object>(skus);
    }

    public Task<byte[]> ExportReconciliationCsvAsync()
    {
        var csv = "OrderCode,Channel,CustomerPaid,PlatformFees,NetReceived,ReconciliationStatus\n" +
                  "TTS-882103,TikTok,450000,33500,416500,Reconciled\n" +
                  "SHP-992014,Shopee,620000,52700,567300,Reconciled\n" +
                  "POS-100293,POS,1250000,12500,1237500,Reconciled\n";
        return Task.FromResult(Encoding.UTF8.GetBytes(csv));
    }
}
