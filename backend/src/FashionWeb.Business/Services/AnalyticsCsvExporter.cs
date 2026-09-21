using System.Globalization;
using System.Text;
using FashionWeb.Business.Interfaces.Services;
using FashionWeb.Business.Results;

namespace FashionWeb.Business.Services;

public class AnalyticsCsvExporter : IAnalyticsCsvExporter
{
    public byte[] ExportOrders(IEnumerable<DrilldownOrderResult> orders)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Order ID,External Order ID,Sales Channel,Delivered At,Gross Revenue (VND),Total Platform Fees (VND),Projected Settlement (VND),COGS (VND),Contribution Profit (VND),Contribution Margin (%)");

        foreach (var o in orders)
        {
            var line = string.Format(
                CultureInfo.InvariantCulture,
                "{0},{1},{2},{3:yyyy-MM-dd HH:mm:ss},{4:F2},{5:F2},{6:F2},{7:F2},{8:F2},{9:F2}%",
                EscapeCsv(o.Id.ToString()),
                EscapeCsv(o.ExternalOrderId),
                EscapeCsv(o.Channel.ToString()),
                o.DeliveredAt,
                o.GrossRevenue,
                o.TotalPlatformFees,
                o.ProjectedSettlement,
                o.Cogs,
                o.ContributionProfit,
                o.ContributionMarginPct
            );
            sb.AppendLine(line);
        }

        var preamble = Encoding.UTF8.GetPreamble();
        var contentBytes = Encoding.UTF8.GetBytes(sb.ToString());
        var result = new byte[preamble.Length + contentBytes.Length];
        Buffer.BlockCopy(preamble, 0, result, 0, preamble.Length);
        Buffer.BlockCopy(contentBytes, 0, result, preamble.Length, contentBytes.Length);

        return result;
    }

    private static string EscapeCsv(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return "\"\"";

        var escaped = value.Replace("\"", "\"\"");
        return $"\"{escaped}\"";
    }
}
