using FashionWeb.Business.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace FashionWeb.Api.Controllers;

public class SettlementController : BaseApiController
{
    private readonly ISettlementService _settlementService;

    public SettlementController(ISettlementService settlementService)
    {
        _settlementService = settlementService;
    }

    [HttpGet("ledger")]
    public async Task<IActionResult> GetLedger([FromQuery] string? channel, [FromQuery] string? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _settlementService.GetLedgerAsync(channel, status, page, pageSize);
        return Ok(result);
    }

    [HttpPost("statements/import")]
    public async Task<IActionResult> ImportStatement([FromForm] IFormFile file, [FromForm] string channelCode)
    {
        if (file == null || file.Length == 0)
            return BadRequest("Vui lòng tải lên tệp sao kê hợp lệ (.xlsx, .csv).");

        using var stream = file.OpenReadStream();
        var report = await _settlementService.ImportStatementAsync(stream, file.FileName, channelCode);
        return Ok(report);
    }
}
