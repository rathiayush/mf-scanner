using Microsoft.AspNetCore.Mvc;
using MutualFundScanner.Application.DTOs;
using MutualFundScanner.Application.Interfaces;

namespace MutualFundScanner.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SnapshotManagementController : ControllerBase
{
    private readonly ISnapshotManagementService _snapshotManagementService;
    private readonly ILogger<SnapshotManagementController> _logger;

    public SnapshotManagementController(
        ISnapshotManagementService snapshotManagementService,
        ILogger<SnapshotManagementController> logger)
    {
        _snapshotManagementService = snapshotManagementService;
        _logger = logger;
    }

    /// <summary>
    /// Get all snapshots for a specific mutual fund
    /// </summary>
    /// <param name="mutualFundId">The ID of the mutual fund</param>
    /// <returns>List of snapshots with basic information</returns>
    [HttpGet("snapshots/{mutualFundId}")]
    public async Task<ActionResult<SnapshotListResponseDto>> GetSnapshots(int mutualFundId)
    {
        try
        {
            _logger.LogInformation("Getting snapshots for mutual fund ID: {MutualFundId}", mutualFundId);

            var request = new SnapshotListRequestDto { MutualFundId = mutualFundId };
            var result = await _snapshotManagementService.GetSnapshotsForMutualFundAsync(request);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting snapshots for mutual fund ID: {MutualFundId}", mutualFundId);
            return StatusCode(500, new SnapshotListResponseDto
            {
                Success = false,
                Message = "Internal server error occurred while retrieving snapshots"
            });
        }
    }

    /// <summary>
    /// Get detailed holdings for a specific snapshot
    /// </summary>
    /// <param name="request">Request containing mutual fund ID, snapshot date, and pagination/sorting parameters</param>
    /// <returns>Detailed holdings with pagination</returns>
    [HttpPost("holdings")]
    public async Task<ActionResult<SnapshotHoldingsResponseDto>> GetSnapshotHoldings([FromBody] SnapshotHoldingsRequestDto request)
    {
        try
        {
            _logger.LogInformation("Getting holdings for mutual fund ID: {MutualFundId}, Date: {Date}, Page: {Page}, PageSize: {PageSize}",
                request.MutualFundId, request.SnapshotDate, request.Page, request.PageSize);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _snapshotManagementService.GetSnapshotHoldingsAsync(request);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting holdings for mutual fund ID: {MutualFundId}, Date: {Date}",
                request.MutualFundId, request.SnapshotDate);
            return StatusCode(500, new SnapshotHoldingsResponseDto
            {
                Success = false,
                Message = "Internal server error occurred while retrieving holdings"
            });
        }
    }

    /// <summary>
    /// Hard delete a snapshot and all related data
    /// </summary>
    /// <param name="request">Request containing mutual fund ID and snapshot date</param>
    /// <returns>Deletion result with summary of deleted records</returns>
    [HttpDelete("snapshot")]
    public async Task<ActionResult<DeleteSnapshotResponseDto>> DeleteSnapshot([FromBody] DeleteSnapshotRequestDto request)
    {
        try
        {
            _logger.LogInformation("Deleting snapshot for mutual fund ID: {MutualFundId}, Date: {Date}",
                request.MutualFundId, request.SnapshotDate);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _snapshotManagementService.DeleteSnapshotAsync(request);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting snapshot for mutual fund ID: {MutualFundId}, Date: {Date}",
                request.MutualFundId, request.SnapshotDate);
            return StatusCode(500, new DeleteSnapshotResponseDto
            {
                Success = false,
                Message = "Internal server error occurred while deleting snapshot"
            });
        }
    }

    /// <summary>
    /// Bulk delete snapshots for multiple mutual funds on a specific date
    /// </summary>
    /// <param name="request">Request containing mutual fund IDs and snapshot date</param>
    /// <returns>Bulk deletion result with detailed summary</returns>
    [HttpDelete("snapshots/bulk")]
    public async Task<ActionResult<BulkDeleteSnapshotResponseDto>> BulkDeleteSnapshots([FromBody] BulkDeleteSnapshotRequestDto request)
    {
        try
        {
            _logger.LogInformation("Bulk deleting snapshots for {Count} mutual funds on date: {Date}",
                request.MutualFundIds.Count, request.SnapshotDate);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _snapshotManagementService.BulkDeleteSnapshotsAsync(request);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in bulk delete snapshots for {Count} mutual funds on date: {Date}",
                request.MutualFundIds.Count, request.SnapshotDate);
            return StatusCode(500, new BulkDeleteSnapshotResponseDto
            {
                Success = false,
                Message = "Internal server error occurred during bulk deletion"
            });
        }
    }

    /// <summary>
    /// Get available snapshot dates that exist for all selected mutual funds
    /// </summary>
    /// <param name="request">Request containing mutual fund IDs</param>
    /// <returns>List of dates that have snapshots for all selected funds</returns>
    [HttpPost("available-dates")]
    public async Task<ActionResult<AvailableSnapshotDatesResponseDto>> GetAvailableSnapshotDates([FromBody] AvailableSnapshotDatesRequestDto request)
    {
        try
        {
            _logger.LogInformation("Getting available snapshot dates for {Count} mutual funds", request.MutualFundIds.Count);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _snapshotManagementService.GetAvailableSnapshotDatesAsync(request);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting available snapshot dates for {Count} mutual funds", request.MutualFundIds.Count);
            return StatusCode(500, new AvailableSnapshotDatesResponseDto
            {
                Success = false,
                Message = "Internal server error occurred while retrieving available dates"
            });
        }
    }
}
