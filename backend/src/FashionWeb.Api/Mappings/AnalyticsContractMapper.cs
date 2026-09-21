using FashionWeb.Api.Contracts.Analytics;
using FashionWeb.Business.Results;

namespace FashionWeb.Api.Mappings;

public static class AnalyticsContractMapper
{
    public static FinancialKpiResponse MapToFinancialKpiResponse(FinancialKpiResult kpis)
    {
        return new FinancialKpiResponse(
            GrossRevenue: kpis.GrossRevenue,
            TotalPlatformFees: kpis.TotalPlatformFees,
            ProjectedSettlement: kpis.ProjectedSettlement,
            Cogs: kpis.Cogs,
            ContributionProfit: kpis.ContributionProfit,
            ContributionMarginPct: kpis.ContributionMarginPct,
            DeliveredOrderCount: kpis.DeliveredOrderCount
        );
    }

    public static FinancialTrendResponse MapToFinancialTrendResponse(IEnumerable<FinancialTrendPointResult> points)
    {
        var dtos = points.Select(p => new FinancialTrendPoint(
            Date: p.Date,
            GrossRevenue: p.GrossRevenue,
            ContributionProfit: p.ContributionProfit
        )).ToList();

        return new FinancialTrendResponse(dtos);
    }

    public static ChannelBreakdownListResponse MapToChannelBreakdownListResponse(IEnumerable<ChannelBreakdownResult> channels)
    {
        var dtos = channels.Select(c => new ChannelBreakdownResponse(
            Channel: c.Channel,
            GrossRevenue: c.GrossRevenue,
            DeliveredOrderCount: c.DeliveredOrderCount,
            TotalPlatformFees: c.TotalPlatformFees,
            ProjectedSettlement: c.ProjectedSettlement,
            Cogs: c.Cogs,
            ContributionProfit: c.ContributionProfit,
            ContributionMarginPct: c.ContributionMarginPct
        )).ToList();

        return new ChannelBreakdownListResponse(dtos);
    }

    public static TopSkuListResponse MapToTopSkuListResponse(IEnumerable<TopSkuResult> skus)
    {
        var dtos = skus.Select(s => new TopSkuResponse(
            SkuCode: s.SkuCode,
            ProductName: s.ProductName,
            QuantitySold: s.QuantitySold,
            GrossRevenue: s.GrossRevenue,
            AllocatedPlatformFees: s.AllocatedPlatformFees,
            Cogs: s.Cogs,
            ContributionProfit: s.ContributionProfit,
            ContributionMarginPct: s.ContributionMarginPct
        )).ToList();

        return new TopSkuListResponse(dtos);
    }

    public static PagedDrilldownOrderResponse MapToPagedDrilldownResponse(PagedResult<DrilldownOrderResult> paged)
    {
        var items = paged.Items.Select(d => new DrilldownOrderItem(
            OrderId: d.OrderId,
            ExternalOrderId: d.ExternalOrderId,
            Channel: d.Channel,
            DeliveredAt: d.DeliveredAt,
            GrossRevenue: d.GrossRevenue,
            TotalPlatformFees: d.TotalPlatformFees,
            ProjectedSettlement: d.ProjectedSettlement,
            Cogs: d.Cogs,
            ContributionProfit: d.ContributionProfit,
            ContributionMarginPct: d.ContributionMarginPct
        )).ToList();

        return new PagedDrilldownOrderResponse(items, paged.Page, paged.PageSize, paged.TotalItems, paged.TotalPages);
    }
}
