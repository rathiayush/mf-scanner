using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MutualFundScanner.Application.DTOs;
using MutualFundScanner.Application.Interfaces;
using MutualFundScanner.Application.Services;

namespace MutualFundScanner.API.Controllers;

[ApiController]
[Route("api/uploadv2")]
[Authorize(Roles = "Admin")]
public class UploadV2Controller : ControllerBase
{
    private readonly IStreamlinedUploadService _uploadService;
    private readonly ILogger<UploadV2Controller> _logger;

    public UploadV2Controller(
        IStreamlinedUploadService uploadService,
        ILogger<UploadV2Controller> logger)
    {
        _uploadService = uploadService;
        _logger = logger;
    }

    [HttpPost("upload")]
    public async Task<IActionResult> Upload([FromForm] UploadV2RequestDto request)
    {
        try
        {
            _logger.LogInformation("UploadV2/Upload endpoint called");
            var result = await _uploadService.ProcessUploadAsync(request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing upload");
            return StatusCode(500, new { Message = ex.Message });
        }
    }
}
