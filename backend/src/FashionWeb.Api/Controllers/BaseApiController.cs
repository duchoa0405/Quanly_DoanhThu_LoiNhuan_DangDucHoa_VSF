using Microsoft.AspNetCore.Mvc;

namespace FashionWeb.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public abstract class BaseApiController : ControllerBase
{
}
