using Microsoft.AspNetCore.Mvc;
using MutualFundScanner.Application.Interfaces;
using MutualFundScanner.Application.DTOs;

namespace MutualFundScanner.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SecurityAnalyticsController : ControllerBase
{
    private readonly ISecurityAnalyticsService _securityAnalyticsService;
    private readonly ILogger<SecurityAnalyticsController> _logger;

    public SecurityAnalyticsController(
        ISecurityAnalyticsService securityAnalyticsService,
        ILogger<SecurityAnalyticsController> logger)
    {
        _securityAnalyticsService = securityAnalyticsService;
        _logger = logger;
    }

    [HttpGet("available-securities")]
    public async Task<IActionResult> GetAvailableSecurities([FromQuery] string? securityNameSearch = null)
    {
        try
        {
            _logger.LogInformation("GetAvailableSecurities called with securityNameSearch: {SecurityNameSearch}", securityNameSearch);
            var result = await _securityAnalyticsService.GetAvailableSecuritiesAsync(securityNameSearch);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting available securities");
            return StatusCode(500, new { Message = ex.Message });
        }
    }

    [HttpGet("securities-by-isin")]
    public async Task<IActionResult> GetSecuritiesByIsin([FromQuery] string? isinSearch = null)
    {
        try
        {
            _logger.LogInformation("GetSecuritiesByIsin called with isinSearch: {IsinSearch}", isinSearch);
            var result = await _securityAnalyticsService.GetSecuritiesByIsinAsync(isinSearch);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting securities by ISIN");
            return StatusCode(500, new { Message = ex.Message });
        }
    }

    [HttpPost("analyze")]
    public async Task<IActionResult> GetSecurityAnalytics([FromBody] SecurityAnalyticsRequestDto request)
    {
        try
        {
            _logger.LogInformation("GetSecurityAnalytics called with ISIN: {ISIN}, SecurityName: {SecurityName}, fromDate: {FromDate}, toDate: {ToDate}, page: {Page}, pageSize: {PageSize}, sortBy: {SortBy}, sortDirection: {SortDirection}",
                request.ISIN, request.SecurityName, request.FromDate, request.ToDate, request.Page, request.PageSize, request.SortBy ?? "default", request.SortDirection ?? "desc");

            if (string.IsNullOrEmpty(request.ISIN) && string.IsNullOrEmpty(request.SecurityName))
            {
                return BadRequest(new { Message = "Either ISIN or SecurityName must be provided" });
            }

            if (request.FromDate >= request.ToDate)
            {
                return BadRequest(new { Message = "From date must be before to date" });
            }

            var result = await _securityAnalyticsService.GetSecurityAnalyticsAsync(request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting security analytics for ISIN: {ISIN}, SecurityName: {SecurityName}",
                request.ISIN, request.SecurityName);
            return StatusCode(500, new { Message = ex.Message });
        }
    }
}
