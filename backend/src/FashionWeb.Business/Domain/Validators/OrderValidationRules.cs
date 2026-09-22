using FashionWeb.Business.Commands;
using FashionWeb.Business.Domain.Entities;
using FashionWeb.Business.Domain.Enums;
using FashionWeb.Business.Exceptions;

namespace FashionWeb.Business.Domain.Validators;

public static class OrderValidationRules
{
    public static void ValidateChannelPaymentCompatibility(ChannelType channel, PaymentMethod paymentMethod)
    {
        switch (channel)
        {
            case ChannelType.TIKTOK:
            case ChannelType.SHOPEE:
                if (paymentMethod != PaymentMethod.MARKETPLACE_WALLET)
                {
                    throw new ValidationException(
                        $"Channel '{channel}' is incompatible with payment method '{paymentMethod}'. Marketplace channels only support '{PaymentMethod.MARKETPLACE_WALLET}'.");
                }
                break;

            case ChannelType.POS:
                if (paymentMethod == PaymentMethod.MARKETPLACE_WALLET)
                {
                    throw new ValidationException(
                        $"Channel 'POS' is incompatible with payment method '{paymentMethod}'. In-store POS only supports '{PaymentMethod.CASH}' or '{PaymentMethod.POS_CARD_QR}'.");
                }
                break;

            default:
                throw new ValidationException($"Unsupported channel type '{channel}'.");
        }
    }

    public static void ValidateItems(IReadOnlyList<CreateOrderItemCommand> items)
    {
        if (items == null || items.Count == 0)
            throw new ValidationException("An order must contain at least one order item.");

        var duplicate = items.GroupBy(i => i.ProductVariantId).FirstOrDefault(g => g.Count() > 1);
        if (duplicate != null)
        {
            throw new ValidationException(
                $"Duplicate product variant ID '{duplicate.Key}' found in order items. Consolidate quantities into a single item.");
        }

        foreach (var item in items)
        {
            if (item.Quantity <= 0)
                throw new ValidationException($"Item quantity must be greater than zero. Received: {item.Quantity}.");

            if (item.UnitPrice < 0m)
                throw new ValidationException($"Item unit price cannot be negative. Received: {item.UnitPrice:N2}.");
        }
    }

    public static void ValidateVariantActive(ProductVariant variant)
    {
        if (!variant.IsActive)
        {
            throw new ValidationException($"Product variant '{variant.SkuCode}' is inactive and cannot be ordered.");
        }

        if (variant.Product != null && !variant.Product.IsActive)
        {
            throw new ValidationException(
                $"Parent product '{variant.Product.Name}' for variant '{variant.SkuCode}' is inactive and cannot be ordered.");
        }
    }
}
