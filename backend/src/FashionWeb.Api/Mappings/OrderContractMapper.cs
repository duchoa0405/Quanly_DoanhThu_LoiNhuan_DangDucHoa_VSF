using FashionWeb.Api.Contracts.Orders;
using FashionWeb.Business.Domain.Entities;
using FashionWeb.Business.Results;

namespace FashionWeb.Api.Mappings;

public static class OrderContractMapper
{
    public static OrderDetailResponse MapToOrderDetailResponse(OrderDetailResult result, bool canViewCosts = false)
    {
        return new OrderDetailResponse(
            Id: result.Id,
            ExternalOrderId: result.ExternalOrderId,
            Channel: result.Channel,
            PaymentMethod: result.PaymentMethod,
            Status: result.Status,
            Subtotal: result.Subtotal,
            ShopVoucher: result.ShopVoucher,
            GrossRevenue: result.GrossRevenue,
            CustomerName: result.CustomerName,
            CustomerPhone: result.CustomerPhone,
            OrderDate: result.OrderDate,
            DeliveredAt: result.DeliveredAt,
            CancelledAt: result.CancelledAt,
            CancellationReason: result.CancellationReason,
            CreatedAt: result.CreatedAt,
            UpdatedAt: result.UpdatedAt,
            Cogs: canViewCosts ? result.Cogs : null,
            ContributionProfit: canViewCosts ? result.ContributionProfit : null,
            Items: result.Items.Select(i => new OrderItemResponse(
                Id: i.Id,
                ProductVariantId: i.ProductVariantId,
                SkuCode: i.SkuCodeSnapshot,
                ProductName: i.ProductNameSnapshot,
                Quantity: i.Quantity,
                UnitPrice: i.UnitPrice,
                LineTotal: i.LineTotal,
                UnitCostSnapshot: canViewCosts ? i.UnitCostSnapshot : null,
                TotalCost: canViewCosts ? i.TotalCost : null
            )).ToList(),
            StatusHistory: result.StatusHistory.Select(h => new OrderStatusHistoryResponse(
                Id: h.Id,
                FromStatus: h.FromStatus,
                ToStatus: h.ToStatus,
                Reason: h.Reason,
                ChangedBy: h.ChangedBy,
                ChangedAt: h.ChangedAt
            )).ToList(),
            FeeSnapshot: result.FeeSnapshot != null ? new FeeSnapshotResponse(
                CommissionFee: result.FeeSnapshot.CommissionFeeAmount,
                PaymentFee: result.FeeSnapshot.PaymentFeeAmount,
                ServiceFee: result.FeeSnapshot.ServiceFeeAmount,
                FixedFee: result.FeeSnapshot.FixedFeeAmount,
                TotalPlatformFees: result.FeeSnapshot.TotalPlatformFees,
                ProjectedSettlement: result.FeeSnapshot.ProjectedSettlement,
                SnapshottedAt: result.FeeSnapshot.SnapshotAt
            ) : null
        );
    }

    public static OrderListItemResponse MapToOrderListItemResponse(Order o)
    {
        return new OrderListItemResponse(
            Id: o.Id,
            ExternalOrderId: o.ExternalOrderId,
            Channel: o.Channel,
            PaymentMethod: o.PaymentMethod,
            Status: o.Status,
            OrderDate: o.OrderDate,
            CustomerName: o.CustomerName,
            CustomerPhone: o.CustomerPhone,
            Subtotal: o.Subtotal,
            ShopVoucher: o.ShopVoucher,
            GrossRevenue: o.GrossRevenue,
            ItemCount: o.Items.Sum(i => i.Quantity),
            ItemsSummary: o.Items.Select(i => new OrderItemSummary(i.SkuCodeSnapshot, i.Quantity)).ToList(),
            DeliveredAt: o.DeliveredAt,
            CancelledAt: o.CancelledAt,
            CreatedAt: o.CreatedAt
        );
    }

    public static OrderResponse MapToOrderResponse(Order o)
    {
        return new OrderResponse(
            Id: o.Id,
            ExternalOrderId: o.ExternalOrderId,
            Channel: o.Channel,
            PaymentMethod: o.PaymentMethod,
            Status: o.Status,
            Subtotal: o.Subtotal,
            ShopVoucher: o.ShopVoucher,
            GrossRevenue: o.GrossRevenue,
            CustomerName: o.CustomerName,
            CustomerPhone: o.CustomerPhone,
            OrderDate: o.OrderDate,
            DeliveredAt: o.DeliveredAt,
            CancelledAt: o.CancelledAt,
            CancellationReason: o.CancellationReason,
            CreatedAt: o.CreatedAt,
            UpdatedAt: o.UpdatedAt
        );
    }
}
