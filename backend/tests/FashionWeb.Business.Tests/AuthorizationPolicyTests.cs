using System.Security.Claims;
using FashionWeb.Api.Authorization;
using FashionWeb.Api.Mappers;
using FashionWeb.Business.Domain.Entities;
using FashionWeb.Business.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace FashionWeb.Business.Tests;

public class AuthorizationPolicyTests
{
    private readonly IAuthorizationService _authService;

    public AuthorizationPolicyTests()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAuthorization(options =>
        {
            options.AddPolicy(Policies.RequireSalesOps, policy =>
                policy.RequireRole(Roles.SalesOps, Roles.ShopOwner));
            options.AddPolicy(Policies.RequireFinanceManager, policy =>
                policy.RequireRole(Roles.FinanceManager, Roles.ShopOwner));
            options.AddPolicy(Policies.RequireShopOwner, policy =>
                policy.RequireRole(Roles.ShopOwner));
            options.AddPolicy(Policies.RequireOrderViewer, policy =>
                policy.RequireRole(Roles.SalesOps, Roles.FinanceManager, Roles.ShopOwner));
        });

        var sp = services.BuildServiceProvider();
        _authService = sp.GetRequiredService<IAuthorizationService>();
    }

    private static ClaimsPrincipal CreatePrincipal(string? role)
    {
        if (string.IsNullOrEmpty(role))
            return new ClaimsPrincipal(new ClaimsIdentity());

        var identity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Name, "test_user"),
            new Claim(ClaimTypes.Role, role)
        }, "TestAuth");

        return new ClaimsPrincipal(identity);
    }

    [Fact]
    public async Task SalesOps_CanAccess_SalesOpsAndOrderViewer_Only()
    {
        var user = CreatePrincipal(Roles.SalesOps);

        var canSalesOps = await _authService.AuthorizeAsync(user, Policies.RequireSalesOps);
        var canViewOrders = await _authService.AuthorizeAsync(user, Policies.RequireOrderViewer);
        var canFinance = await _authService.AuthorizeAsync(user, Policies.RequireFinanceManager);
        var canOwner = await _authService.AuthorizeAsync(user, Policies.RequireShopOwner);

        Assert.True(canSalesOps.Succeeded, "SalesOps must satisfy RequireSalesOps");
        Assert.True(canViewOrders.Succeeded, "SalesOps must satisfy RequireOrderViewer");
        Assert.False(canFinance.Succeeded, "SalesOps must NOT satisfy RequireFinanceManager");
        Assert.False(canOwner.Succeeded, "SalesOps must NOT satisfy RequireShopOwner");
    }

    [Fact]
    public async Task FinanceManager_CanAccess_FinanceManagerAndOrderViewer_Only()
    {
        var user = CreatePrincipal(Roles.FinanceManager);

        var canFinance = await _authService.AuthorizeAsync(user, Policies.RequireFinanceManager);
        var canViewOrders = await _authService.AuthorizeAsync(user, Policies.RequireOrderViewer);
        var canSalesOps = await _authService.AuthorizeAsync(user, Policies.RequireSalesOps);
        var canOwner = await _authService.AuthorizeAsync(user, Policies.RequireShopOwner);

        Assert.True(canFinance.Succeeded, "FinanceManager must satisfy RequireFinanceManager");
        Assert.True(canViewOrders.Succeeded, "FinanceManager must satisfy RequireOrderViewer");
        Assert.False(canSalesOps.Succeeded, "FinanceManager must NOT satisfy RequireSalesOps (Orders are read-only for Finance)");
        Assert.False(canOwner.Succeeded, "FinanceManager must NOT satisfy RequireShopOwner");
    }

    [Fact]
    public async Task ShopOwner_HasUnrestrictedAccess_ToAllPolicies()
    {
        var user = CreatePrincipal(Roles.ShopOwner);

        var canSalesOps = await _authService.AuthorizeAsync(user, Policies.RequireSalesOps);
        var canFinance = await _authService.AuthorizeAsync(user, Policies.RequireFinanceManager);
        var canOwner = await _authService.AuthorizeAsync(user, Policies.RequireShopOwner);
        var canViewOrders = await _authService.AuthorizeAsync(user, Policies.RequireOrderViewer);

        Assert.True(canSalesOps.Succeeded, "ShopOwner must satisfy RequireSalesOps");
        Assert.True(canFinance.Succeeded, "ShopOwner must satisfy RequireFinanceManager");
        Assert.True(canOwner.Succeeded, "ShopOwner must satisfy RequireShopOwner");
        Assert.True(canViewOrders.Succeeded, "ShopOwner must satisfy RequireOrderViewer");
    }

    [Fact]
    public async Task UnauthorizedRole_IsRejected_FromAllPolicies()
    {
        var user = CreatePrincipal("WarehouseStaff");

        var canSalesOps = await _authService.AuthorizeAsync(user, Policies.RequireSalesOps);
        var canFinance = await _authService.AuthorizeAsync(user, Policies.RequireFinanceManager);
        var canOwner = await _authService.AuthorizeAsync(user, Policies.RequireShopOwner);
        var canViewOrders = await _authService.AuthorizeAsync(user, Policies.RequireOrderViewer);

        Assert.False(canSalesOps.Succeeded);
        Assert.False(canFinance.Succeeded);
        Assert.False(canOwner.Succeeded);
        Assert.False(canViewOrders.Succeeded);
    }

    [Fact]
    public void OrderContractMapper_HidesCostAndProfit_WhenUserLacksCostPermission()
    {
        var order = new Order
        {
            ExternalOrderId = "ORD-RBAC-001",
            Channel = ChannelType.TIKTOK,
            PaymentMethod = PaymentMethod.MARKETPLACE_WALLET,
        };
        order.SetFinancials(500000m, 50000m);
        order.Items.Add(new OrderItem
        {
            SkuCode = "TSHIRT-BLK",
            Quantity = 2,
            UnitPrice = 250000m,
            UnitCostSnapshot = 100000m
        });
        order.MarkAsPending("system");
        order.TransitionToShipped("system");
        order.TransitionToDelivered("system");

        // Act: Map with canViewCosts = false (SalesOps perspective)
        var salesOpsDto = OrderContractMapper.ToDetailResponse(order, canViewCosts: false);

        // Assert: Costs & Contribution Profit must be completely null / hidden
        Assert.Null(salesOpsDto.Cogs);
        Assert.Null(salesOpsDto.ContributionProfit);
        Assert.Null(salesOpsDto.Items[0].UnitCostSnapshot);

        // Act: Map with canViewCosts = true (FinanceManager / ShopOwner perspective)
        var financeDto = OrderContractMapper.ToDetailResponse(order, canViewCosts: true);

        // Assert: Costs & Contribution Profit must be accurately populated
        Assert.NotNull(financeDto.Cogs);
        Assert.Equal(200000m, financeDto.Cogs);
        Assert.NotNull(financeDto.ContributionProfit);
        Assert.Equal(100000m, financeDto.Items[0].UnitCostSnapshot);
    }
}
