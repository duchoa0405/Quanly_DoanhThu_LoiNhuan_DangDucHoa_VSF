using FashionWeb.Business.Results;

namespace FashionWeb.Business.Interfaces.Services;

public interface IAnalyticsCsvExporter
{
    byte[] ExportOrders(IEnumerable<DrilldownOrderResult> orders);
}
