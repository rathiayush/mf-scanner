using Microsoft.AspNetCore.Mvc;
using MutualFundScanner.Application.Interfaces;
using MutualFundScanner.Application.DTOs;

namespace MutualFundScanner.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AnalyticsController : ControllerBase
{
    private readonly IAnalyticsService _analyticsService;
    private readonly ILogger<AnalyticsController> _logger;

    public AnalyticsController(
        IAnalyticsService analyticsService,
        ILogger<AnalyticsController> logger)
    {
        _analyticsService = analyticsService;
        _logger = logger;
    }

    [HttpPost("analyze")]
    public async Task<IActionResult> GetAnalytics([FromBody] AnalyticsRequestDto request)
    {
        try
        {
            if (!request.MutualFundIds.Any())
            {
                return BadRequest(new { Message = "At least one mutual fund must be selected" });
            }

            if (request.FromDate >= request.ToDate)
            {
                return BadRequest(new { Message = "From date must be before to date" });
            }

            var result = await _analyticsService.GetAnalyticsAsync(request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting analytics for funds {FundIds}", string.Join(",", request.MutualFundIds));
            return StatusCode(500, new { Message = ex.Message });
        }
    }

    [HttpPost("holding-changes")]
    public async Task<IActionResult> GetHoldingChanges([FromBody] AnalyticsRequestDto request)
    {
        try
        {
            if (!request.MutualFundIds.Any())
            {
                return BadRequest(new { Message = "At least one mutual fund must be selected" });
            }

            var result = await _analyticsService.GetHoldingChangesAsync(request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting holding changes for funds {FundIds}", string.Join(",", request.MutualFundIds));
            return StatusCode(500, new { Message = ex.Message });
        }
    }

    [HttpGet("top-shares")]
    public async Task<IActionResult> GetTopShares(
        [FromQuery] int[] mutualFundIds,
        [FromQuery] DateTime fromDate,
        [FromQuery] DateTime toDate,
        [FromQuery] int topCount = 15)
    {
        try
        {
            if (!mutualFundIds.Any())
            {
                return BadRequest(new { Message = "At least one mutual fund must be selected" });
            }

            if (fromDate >= toDate)
            {
                return BadRequest(new { Message = "From date must be before to date" });
            }

            var result = await _analyticsService.GetTopSharesAsync(mutualFundIds.ToList(), fromDate, toDate, topCount);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting top shares for funds {FundIds}", string.Join(",", mutualFundIds));
            return StatusCode(500, new { Message = ex.Message });
        }
    }

    [HttpGet("available-dates")]
    public async Task<IActionResult> GetAvailableDates([FromQuery] int[]? mutualFundIds = null)
    {
        try
        {
            var result = await _analyticsService.GetAvailableDatesAsync(mutualFundIds?.ToList());
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting available dates");
            return StatusCode(500, new { Message = ex.Message });
        }
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetAnalyticsSummary(
        [FromQuery] int[] mutualFundIds,
        [FromQuery] DateTime fromDate,
        [FromQuery] DateTime toDate)
    {
        try
        {
            if (!mutualFundIds.Any())
            {
                return BadRequest(new { Message = "At least one mutual fund must be selected" });
            }

            if (fromDate >= toDate)
            {
                return BadRequest(new { Message = "From date must be before to date" });
            }

            var result = await _analyticsService.GetAnalyticsSummaryAsync(mutualFundIds.ToList(), fromDate, toDate);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting analytics summary for funds {FundIds}", string.Join(",", mutualFundIds));
            return StatusCode(500, new { Message = ex.Message });
        }
    }

    [HttpPost("current-holdings")]
    public async Task<IActionResult> GetCurrentHoldings([FromBody] CurrentHoldingsRequestDto request)
    {
        try
        {
            if (request.MutualFundId <= 0)
            {
                return BadRequest(new { Message = "Valid mutual fund ID is required" });
            }

            if (request.Date == default)
            {
                return BadRequest(new { Message = "Valid date is required" });
            }

            var result = await _analyticsService.GetCurrentHoldingsAsync(request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current holdings for fund {FundId} on date {Date}", 
                request.MutualFundId, request.Date);
            return StatusCode(500, new { Message = ex.Message });
        }
    }
}
