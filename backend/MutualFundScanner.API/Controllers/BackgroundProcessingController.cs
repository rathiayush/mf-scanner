using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MutualFundScanner.Application.DTOs;
using MutualFundScanner.Application.Interfaces;
using MutualFundScanner.Domain.Enums;

namespace MutualFundScanner.API.Controllers;

[ApiController]
[Route("api/background-processing")]
[Authorize(Roles = "Admin")]
public class BackgroundProcessingController : ControllerBase
{
    private readonly IBackgroundProcessingService _backgroundProcessingService;
    private readonly ILogger<BackgroundProcessingController> _logger;

    public BackgroundProcessingController(
        IBackgroundProcessingService backgroundProcessingService,
        ILogger<BackgroundProcessingController> logger)
    {
        _backgroundProcessingService = backgroundProcessingService;
        _logger = logger;
    }

    /// <summary>
    /// Starts background processing of files from a directory
    /// </summary>
    /// <param name="request">Processing configuration</param>
    /// <returns>Job ID for tracking</returns>
    [HttpPost("start")]
    public async Task<IActionResult> StartProcessing([FromBody] BackgroundProcessingRequestDto request)
    {
        try
        {
            _logger.LogInformation("Starting background processing for directory: {Directory}", 
                request.DirectoryPath ?? "default");

            var jobId = await _backgroundProcessingService.StartBackgroundProcessingAsync(request);
            
            return Ok(new { JobId = jobId, Message = "Background processing started successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting background processing");
            return StatusCode(500, new { Message = ex.Message });
        }
    }

    /// <summary>
    /// Gets the status of a specific background processing job
    /// </summary>
    /// <param name="jobId">Job ID</param>
    /// <returns>Job status and progress</returns>
    [HttpGet("status/{jobId}")]
    public async Task<IActionResult> GetJobStatus(string jobId)
    {
        try
        {
            var status = await _backgroundProcessingService.GetJobStatusAsync(jobId);
            
            if (status == null)
            {
                return NotFound(new { Message = "Job not found" });
            }

            return Ok(status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting job status for {JobId}", jobId);
            return StatusCode(500, new { Message = ex.Message });
        }
    }

    /// <summary>
    /// Gets all background processing jobs
    /// </summary>
    /// <returns>List of all jobs with their status</returns>
    [HttpGet("jobs")]
    public async Task<IActionResult> GetAllJobs()
    {
        try
        {
            var jobs = await _backgroundProcessingService.GetAllJobsAsync();
            return Ok(jobs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all jobs");
            return StatusCode(500, new { Message = ex.Message });
        }
    }

    /// <summary>
    /// Cancels a running background processing job
    /// </summary>
    /// <param name="jobId">Job ID</param>
    /// <returns>Cancellation result</returns>
    [HttpPost("cancel/{jobId}")]
    public async Task<IActionResult> CancelJob(string jobId)
    {
        try
        {
            var cancelled = await _backgroundProcessingService.CancelJobAsync(jobId);
            
            if (!cancelled)
            {
                return BadRequest(new { Message = "Job could not be cancelled (may not be running or not found)" });
            }

            return Ok(new { Message = "Job cancelled successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling job {JobId}", jobId);
            return StatusCode(500, new { Message = ex.Message });
        }
    }

    /// <summary>
    /// Downloads the Excel report for a completed job
    /// </summary>
    /// <param name="jobId">Job ID</param>
    /// <returns>Excel file download</returns>
    [HttpGet("report/{jobId}")]
    public async Task<IActionResult> DownloadReport(string jobId)
    {
        try
        {
            var report = await _backgroundProcessingService.GetExcelReportAsync(jobId);
            
            if (report == null)
            {
                return NotFound(new { Message = "Report not found or not yet generated" });
            }

            return File(report.Value.fileBytes, 
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
                report.Value.fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading report for job {JobId}", jobId);
            return StatusCode(500, new { Message = ex.Message });
        }
    }

    /// <summary>
    /// Gets processing statistics summary for dashboard
    /// </summary>
    /// <returns>Summary statistics</returns>
    [HttpGet("summary")]
    public async Task<IActionResult> GetProcessingSummary()
    {
        try
        {
            var jobs = await _backgroundProcessingService.GetAllJobsAsync();
            
            var summary = new
            {
                TotalJobs = jobs.Count,
                RunningJobs = jobs.Count(j => j.Status == BackgroundJobStatus.Running),
                CompletedJobs = jobs.Count(j => j.Status == BackgroundJobStatus.Completed),
                FailedJobs = jobs.Count(j => j.Status == BackgroundJobStatus.Failed),
                QueuedJobs = jobs.Count(j => j.Status == BackgroundJobStatus.Queued),
                RecentJobs = jobs.Take(5).Select(j => new
                {
                    j.JobId,
                    j.Status,
                    j.StartTime,
                    j.Duration,
                    j.TotalFiles,
                    j.SuccessfulFiles,
                    j.FailedFiles
                }).ToList()
            };

            return Ok(summary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting processing summary");
            return StatusCode(500, new { Message = ex.Message });
        }
    }

    /// <summary>
    /// Validates a directory path for processing
    /// </summary>
    /// <param name="directoryPath">Directory path to validate</param>
    /// <returns>Validation result with file count</returns>
    [HttpPost("validate-directory")]
    public IActionResult ValidateDirectory([FromBody] string directoryPath)
    {
        try
        {
            if (string.IsNullOrEmpty(directoryPath))
            {
                return BadRequest(new { Message = "Directory path is required" });
            }

            if (!Directory.Exists(directoryPath))
            {
                return BadRequest(new { Message = "Directory does not exist" });
            }

            var excelFiles = Directory.GetFiles(directoryPath, "*.xlsx", SearchOption.TopDirectoryOnly)
                .Concat(Directory.GetFiles(directoryPath, "*.xls", SearchOption.TopDirectoryOnly))
                .ToList();

            var result = new
            {
                IsValid = true,
                DirectoryPath = directoryPath,
                ExcelFileCount = excelFiles.Count,
                Files = excelFiles.Select(f => new
                {
                    FileName = Path.GetFileName(f),
                    FilePath = f,
                    FileSize = new FileInfo(f).Length,
                    LastModified = new FileInfo(f).LastWriteTime
                }).OrderBy(f => f.FileName).ToList()
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating directory {DirectoryPath}", directoryPath);
            return StatusCode(500, new { Message = ex.Message });
        }
    }
}
