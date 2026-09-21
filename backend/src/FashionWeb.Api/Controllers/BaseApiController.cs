using Microsoft.AspNetCore.Mvc;

namespace FashionWeb.Api.Controllers;

[ApiController]
public abstract class BaseApiController : ControllerBase
{
    protected string GetCurrentUserIdentity()
    {
        return User.Identity?.Name ?? "system";
    }
}
