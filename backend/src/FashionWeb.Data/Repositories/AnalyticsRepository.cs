using FashionWeb.Business.Common;
using FashionWeb.Business.Domain.Enums;
using FashionWeb.Business.Exceptions;
using FashionWeb.Business.Filters;
using FashionWeb.Business.Interfaces.Repositories;
using FashionWeb.Business.Results;
using FashionWeb.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace FashionWeb.Data.Repositories;

public class AnalyticsRepository : IAnalyticsRepository
{
    private readonly AppDbContext _context;

    public AnalyticsRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<FinancialKpiResult> QueryKpisAsync(AnalyticsFilter filter, CancellationToken ct = default)
    {
        // Strict revenue recognition on DeliveredAt
        var baseQuery = _context.Orders
            .Where(o => o.Status == OrderStatus.DELIVERED && o.DeliveredAt != null);

        if (filter.FromDate.HasValue)
            baseQuery = baseQuery.Where(o => o.DeliveredAt >= filter.FromDate.Value);

        if (filter.ToDate.HasValue)
            baseQuery = baseQuery.Where(o => o.DeliveredAt <= filter.ToDate.Value);

        if (filter.Channel.HasValue)
            baseQuery = baseQuery.Where(o => o.Channel == filter.Channel.Value);

        var count = await baseQuery.CountAsync(ct);
        if (count == 0)
        {
            return new FinancialKpiResult(0.00m, 0.00m, 0.00m, 0.00m, 0.00m, 0.00m, 0);
        }

        var data = await (from o in baseQuery
                          join fs in _context.OrderFeeSnapshots on o.Id equals fs.OrderId into feeSnapshots
                          from fs in feeSnapshots.DefaultIfEmpty()
                          select new
                          {
                              OrderId = o.Id,
                              GrossRevenue = o.GrossRevenue,
                              Snapshot = fs,
                              Cogs = o.Items.Sum(i => i.TotalCost)
                          }).ToListAsync(ct);

        var missing = data.FirstOrDefault(x => x.Snapshot == null);
        if (missing != null)
        {
            throw new BusinessRuleException(
                $"Data integrity violation: DELIVERED order '{missing.OrderId}' is missing an OrderFeeSnapshot. Financial reports cannot be generated with missing fee deductions.");
        }

        var grossRevenue = data.Sum(x => x.GrossRevenue);
        var totalPlatformFees = data.Sum(x => x.Snapshot!.TotalPlatformFees);
        var projectedSettlement = grossRevenue - totalPlatformFees;
        var cogs = data.Sum(x => x.Cogs);
        var contributionProfit = FinancialCalculator.CalculateOrderProfit(grossRevenue, totalPlatformFees, cogs);
        var marginPct = FinancialCalculator.CalculateContributionMargin(contributionProfit, grossRevenue);

        return new FinancialKpiResult(
            GrossRevenue: grossRevenue,
            TotalPlatformFees: totalPlatformFees,
            ProjectedSettlement: projectedSettlement,
            Cogs: cogs,
            ContributionProfit: contributionProfit,
            ContributionMarginPct: marginPct,
            DeliveredOrderCount: count
        );
    }

    public async Task<List<FinancialTrendPointResult>> QueryTrendAsync(TrendFilter filter, CancellationToken ct = default)
    {
        var baseQuery = _context.Orders
            .Where(o => o.Status == OrderStatus.DELIVERED && o.DeliveredAt != null);

        if (filter.FromDate.HasValue)
            baseQuery = baseQuery.Where(o => o.DeliveredAt >= filter.FromDate.Value);

        if (filter.ToDate.HasValue)
            baseQuery = baseQuery.Where(o => o.DeliveredAt <= filter.ToDate.Value);

        if (filter.Channel.HasValue)
            baseQuery = baseQuery.Where(o => o.Channel == filter.Channel.Value);

        var orderData = await (from o in baseQuery
                               join fs in _context.OrderFeeSnapshots on o.Id equals fs.OrderId into feeSnapshots
                               from fs in feeSnapshots.DefaultIfEmpty()
                               select new
                               {
                                   OrderId = o.Id,
                                   Date = DateOnly.FromDateTime(o.DeliveredAt!.Value),
                                   GrossRevenue = o.GrossRevenue,
                                   Snapshot = fs,
                                   Cogs = o.Items.Sum(i => i.TotalCost)
                               }).ToListAsync(ct);

        var missing = orderData.FirstOrDefault(x => x.Snapshot == null);
        if (missing != null)
        {
            throw new BusinessRuleException(
                $"Data integrity violation: DELIVERED order '{missing.OrderId}' is missing an OrderFeeSnapshot. Trend reports cannot be generated with missing fee deductions.");
        }

        var grouped = orderData
            .GroupBy(x => x.Date)
            .ToDictionary(
                g => g.Key,
                g =>
                {
                    var gross = g.Sum(x => x.GrossRevenue);
                    var fees = g.Sum(x => x.Snapshot!.TotalPlatformFees);
                    var cogs = g.Sum(x => x.Cogs);
                    var profit = FinancialCalculator.CalculateOrderProfit(gross, fees, cogs);
                    return (Gross: gross, Profit: profit);
                });

        var result = new List<FinancialTrendPointResult>();
        var start = filter.FromDate.HasValue ? DateOnly.FromDateTime(filter.FromDate.Value) : DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-30));
        var end = filter.ToDate.HasValue ? DateOnly.FromDateTime(filter.ToDate.Value) : DateOnly.FromDateTime(DateTime.UtcNow);

        for (var d = start; d <= end; d = d.AddDays(1))
        {
            if (grouped.TryGetValue(d, out var val))
            {
                result.Add(new FinancialTrendPointResult(d, val.Gross, val.Profit));
            }
            else
            {
                result.Add(new FinancialTrendPointResult(d, 0.00m, 0.00m));
            }
        }

        return result;
    }

    public async Task<List<ChannelBreakdownResult>> QueryChannelBreakdownAsync(AnalyticsFilter filter, CancellationToken ct = default)
    {
        var baseQuery = _context.Orders
            .Where(o => o.Status == OrderStatus.DELIVERED && o.DeliveredAt != null);

        if (filter.FromDate.HasValue)
            baseQuery = baseQuery.Where(o => o.DeliveredAt >= filter.FromDate.Value);

        if (filter.ToDate.HasValue)
            baseQuery = baseQuery.Where(o => o.DeliveredAt <= filter.ToDate.Value);

        if (filter.Channel.HasValue)
            baseQuery = baseQuery.Where(o => o.Channel == filter.Channel.Value);

        var orderData = await (from o in baseQuery
                               join fs in _context.OrderFeeSnapshots on o.Id equals fs.OrderId into feeSnapshots
                               from fs in feeSnapshots.DefaultIfEmpty()
                               select new
                               {
                                   OrderId = o.Id,
                                   Channel = o.Channel,
                                   GrossRevenue = o.GrossRevenue,
                                   Snapshot = fs,
                                   Cogs = o.Items.Sum(i => i.TotalCost)
                               }).ToListAsync(ct);

        var missing = orderData.FirstOrDefault(x => x.Snapshot == null);
        if (missing != null)
        {
            throw new BusinessRuleException(
                $"Data integrity violation: DELIVERED order '{missing.OrderId}' is missing an OrderFeeSnapshot. Channel breakdown reports cannot be generated with missing fee deductions.");
        }

        var channels = filter.Channel.HasValue
            ? new[] { filter.Channel.Value }
            : Enum.GetValues<ChannelType>();

        var result = new List<ChannelBreakdownResult>();
        foreach (var ch in channels)
        {
            var items = orderData.Where(x => x.Channel == ch).ToList();
            var count = items.Count;
            var gross = items.Sum(x => x.GrossRevenue);
            var fees = items.Sum(x => x.Snapshot!.TotalPlatformFees);
            var cogs = items.Sum(x => x.Cogs);
            var profit = FinancialCalculator.CalculateOrderProfit(gross, fees, cogs);
            var marginPct = FinancialCalculator.CalculateContributionMargin(profit, gross);

            result.Add(new ChannelBreakdownResult(ch, count, gross, fees, profit, marginPct));
        }

        return result;
    }

    public async Task<List<TopSkuResult>> QueryTopSkusAsync(TopSkuFilter filter, CancellationToken ct = default)
    {
        var baseQuery = _context.Orders
            .Where(o => o.Status == OrderStatus.DELIVERED && o.DeliveredAt != null);

        if (filter.FromDate.HasValue)
            baseQuery = baseQuery.Where(o => o.DeliveredAt >= filter.FromDate.Value);

        if (filter.ToDate.HasValue)
            baseQuery = baseQuery.Where(o => o.DeliveredAt <= filter.ToDate.Value);

        if (filter.Channel.HasValue)
            baseQuery = baseQuery.Where(o => o.Channel == filter.Channel.Value);

        var itemsQuery = from o in baseQuery
                         join i in _context.OrderItems on o.Id equals i.OrderId
                         join fs in _context.OrderFeeSnapshots on o.Id equals fs.OrderId into feeSnapshots
                         from fs in feeSnapshots.DefaultIfEmpty()
                         select new
                         {
                             OrderId = o.Id,
                             i.SkuCodeSnapshot,
                             i.ProductNameSnapshot,
                             i.Quantity,
                             i.LineTotal,
                             i.TotalCost,
                             OrderSubtotal = o.Subtotal,
                             ShopVoucher = o.ShopVoucher,
                             Snapshot = fs
                         };

        var rawItems = await itemsQuery.ToListAsync(ct);

        var missing = rawItems.FirstOrDefault(x => x.Snapshot == null);
        if (missing != null)
        {
            throw new BusinessRuleException(
                $"Data integrity violation: DELIVERED order '{missing.OrderId}' is missing an OrderFeeSnapshot. Top SKU reports cannot be generated with missing fee deductions.");
        }

        var groupedQuery = rawItems
            .GroupBy(x => new { x.SkuCodeSnapshot, x.ProductNameSnapshot })
            .Select(g =>
            {
                var skuCode = g.Key.SkuCodeSnapshot;
                var productName = g.Key.ProductNameSnapshot;
                var deliveredUnits = g.Sum(x => x.Quantity);
                var cogs = g.Sum(x => x.TotalCost);

                decimal skuGrossRevenue = 0.00m;
                decimal allocatedFees = 0.00m;

                foreach (var item in g)
                {
                    var allocatedVoucher = FinancialCalculator.AllocateVoucher(item.ShopVoucher, item.LineTotal, item.OrderSubtotal);
                    var lineGrossRevenue = FinancialCalculator.CalculateSkuGrossRevenue(item.LineTotal, allocatedVoucher);
                    skuGrossRevenue += lineGrossRevenue;

                    var itemAllocatedFee = FinancialCalculator.AllocateFees(item.Snapshot!.TotalPlatformFees, item.LineTotal, item.OrderSubtotal);
                    allocatedFees += itemAllocatedFee;
                }

                var profit = FinancialCalculator.CalculateSkuProfit(skuGrossRevenue, 0m, allocatedFees, cogs);
                var marginPct = FinancialCalculator.CalculateContributionMargin(profit, skuGrossRevenue);

                return new TopSkuResult(skuCode, productName, deliveredUnits, skuGrossRevenue, cogs, profit, marginPct);
            });

        var sortBy = filter.SortBy?.Trim().ToUpperInvariant();
        var orderedQuery = sortBy switch
        {
            "GROSS_REVENUE" => groupedQuery.OrderByDescending(x => x.GrossRevenue),
            "DELIVERED_UNITS" => groupedQuery.OrderByDescending(x => x.DeliveredUnits),
            _ => groupedQuery.OrderByDescending(x => x.ContributionProfit)
        };

        return orderedQuery.Take(filter.Limit).ToList();
    }

    public async Task<PagedResult<DrilldownOrderResult>> QueryDrilldownAsync(DrilldownFilter filter, CancellationToken ct = default)
    {
        var baseQuery = _context.Orders
            .Where(o => o.Status == OrderStatus.DELIVERED && o.DeliveredAt != null);

        if (filter.FromDate.HasValue)
            baseQuery = baseQuery.Where(o => o.DeliveredAt >= filter.FromDate.Value);

        if (filter.ToDate.HasValue)
            baseQuery = baseQuery.Where(o => o.DeliveredAt <= filter.ToDate.Value);

        if (filter.Channel.HasValue)
            baseQuery = baseQuery.Where(o => o.Channel == filter.Channel.Value);

        var totalItems = await baseQuery.CountAsync(ct);

        var pagedOrders = await (from o in baseQuery
                                 join fs in _context.OrderFeeSnapshots on o.Id equals fs.OrderId into feeSnapshots
                                 from fs in feeSnapshots.DefaultIfEmpty()
                                 orderby o.DeliveredAt descending
                                 select new
                                 {
                                     o.Id,
                                     o.ExternalOrderId,
                                     o.Channel,
                                     DeliveredAt = o.DeliveredAt!.Value,
                                     o.GrossRevenue,
                                     Snapshot = fs,
                                     Cogs = o.Items.Sum(i => i.TotalCost)
                                 })
                                 .Skip((filter.Page - 1) * filter.PageSize)
                                 .Take(filter.PageSize)
                                 .ToListAsync(ct);

        var missing = pagedOrders.FirstOrDefault(x => x.Snapshot == null);
        if (missing != null)
        {
            throw new BusinessRuleException(
                $"Data integrity violation: DELIVERED order '{missing.Id}' is missing an OrderFeeSnapshot. Drilldown reports cannot be generated with missing fee deductions.");
        }

        var items = pagedOrders.Select(x =>
        {
            var fees = x.Snapshot!.TotalPlatformFees;
            var projected = x.Snapshot!.ProjectedSettlement;
            var profit = FinancialCalculator.CalculateOrderProfit(x.GrossRevenue, fees, x.Cogs);
            var marginPct = FinancialCalculator.CalculateContributionMargin(profit, x.GrossRevenue);
            return new DrilldownOrderResult(
                Id: x.Id,
                ExternalOrderId: x.ExternalOrderId,
                Channel: x.Channel,
                DeliveredAt: x.DeliveredAt,
                GrossRevenue: x.GrossRevenue,
                TotalPlatformFees: fees,
                ProjectedSettlement: projected,
                Cogs: x.Cogs,
                ContributionProfit: profit,
                ContributionMarginPct: marginPct
            );
        }).ToList();

        return new PagedResult<DrilldownOrderResult>(items, totalItems, filter.Page, filter.PageSize);
    }

    public async Task<List<DrilldownOrderResult>> QueryRawExportDataAsync(AnalyticsFilter filter, CancellationToken ct = default)
    {
        var baseQuery = _context.Orders
            .Where(o => o.Status == OrderStatus.DELIVERED && o.DeliveredAt != null);

        if (filter.FromDate.HasValue)
            baseQuery = baseQuery.Where(o => o.DeliveredAt >= filter.FromDate.Value);

        if (filter.ToDate.HasValue)
            baseQuery = baseQuery.Where(o => o.DeliveredAt <= filter.ToDate.Value);

        if (filter.Channel.HasValue)
            baseQuery = baseQuery.Where(o => o.Channel == filter.Channel.Value);

        var orders = await (from o in baseQuery
                            join fs in _context.OrderFeeSnapshots on o.Id equals fs.OrderId into feeSnapshots
                            from fs in feeSnapshots.DefaultIfEmpty()
                            orderby o.DeliveredAt descending
                            select new
                            {
                                o.Id,
                                o.ExternalOrderId,
                                o.Channel,
                                DeliveredAt = o.DeliveredAt!.Value,
                                o.GrossRevenue,
                                Snapshot = fs,
                                Cogs = o.Items.Sum(i => i.TotalCost)
                            }).ToListAsync(ct);

        var missing = orders.FirstOrDefault(x => x.Snapshot == null);
        if (missing != null)
        {
            throw new BusinessRuleException(
                $"Data integrity violation: DELIVERED order '{missing.Id}' is missing an OrderFeeSnapshot. Export reports cannot be generated with missing fee deductions.");
        }

        return orders.Select(x =>
        {
            var fees = x.Snapshot!.TotalPlatformFees;
            var projected = x.Snapshot!.ProjectedSettlement;
            var profit = FinancialCalculator.CalculateOrderProfit(x.GrossRevenue, fees, x.Cogs);
            var marginPct = FinancialCalculator.CalculateContributionMargin(profit, x.GrossRevenue);
            return new DrilldownOrderResult(
                Id: x.Id,
                ExternalOrderId: x.ExternalOrderId,
                Channel: x.Channel,
                DeliveredAt: x.DeliveredAt,
                GrossRevenue: x.GrossRevenue,
                TotalPlatformFees: fees,
                ProjectedSettlement: projected,
                Cogs: x.Cogs,
                ContributionProfit: profit,
                ContributionMarginPct: marginPct
            );
        }).ToList();
    }
}
