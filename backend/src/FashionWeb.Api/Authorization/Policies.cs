namespace FashionWeb.Api.Authorization;

public static class Policies
{
    public const string RequireSalesOps = "RequireSalesOps";
    public const string RequireFinanceManager = "RequireFinanceManager";
    public const string RequireShopOwner = "RequireShopOwner";
    public const string RequireOrderViewer = "RequireOrderViewer";
}
