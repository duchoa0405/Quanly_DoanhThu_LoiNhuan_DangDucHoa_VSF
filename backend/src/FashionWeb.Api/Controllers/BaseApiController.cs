using System.Security.Claims;
using FashionWeb.Api.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FashionWeb.Api.Controllers;

[ApiController]
public abstract class BaseApiController : ControllerBase
{
    protected string GetCurrentUserIdentity()
    {
        var identity = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst(ClaimTypes.Name)?.Value
            ?? User.Identity?.Name;

        if (string.IsNullOrWhiteSpace(identity))
        {
            if (User.Identity?.IsAuthenticated == true)
                return "authenticated_user";

            return "system";
        }

        return identity;
    }

    protected bool CanViewCostAndProfit()
    {
        return User.IsInRole(Roles.FinanceManager) || User.IsInRole(Roles.ShopOwner);
    }
}
