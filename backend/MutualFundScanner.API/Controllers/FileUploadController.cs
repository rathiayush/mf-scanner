using Microsoft.AspNetCore.Mvc;
using MutualFundScanner.Application.Interfaces;
using MutualFundScanner.Application.DTOs;

namespace MutualFundScanner.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FileUploadController : ControllerBase
{
    private readonly IFileUploadService _fileUploadService;
    private readonly ILogger<FileUploadController> _logger;

    public FileUploadController(
        IFileUploadService fileUploadService,
        ILogger<FileUploadController> logger)
    {
        _fileUploadService = fileUploadService;
        _logger = logger;
    }

    [HttpPost("single")]
    public async Task<IActionResult> UploadFiles([FromForm] FileUploadRequestDto request)
    {
        try
        {
            if (!request.Files.Any())
            {
                return BadRequest(new { Message = "No files provided" });
            }

            if (request.MutualFundId <= 0)
            {
                return BadRequest(new { Message = "Valid mutual fund ID is required" });
            }

            var result = await _fileUploadService.UploadFilesAsync(request);
            
            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading files for mutual fund {MutualFundId}", request.MutualFundId);
            return StatusCode(500, new { Message = ex.Message });
        }
    }

    [HttpPost("multiple")]
    public async Task<IActionResult> UploadMultipleFiles([FromForm] MultipleFileUploadRequestDto request)
    {
        try
        {
            if (!request.FileUploads.Any())
            {
                return BadRequest(new { Message = "No file uploads provided" });
            }

            // Validate each file upload
            foreach (var fileUpload in request.FileUploads)
            {
                if (fileUpload.MutualFundId <= 0)
                {
                    return BadRequest(new { Message = $"Valid mutual fund ID is required for file {fileUpload.File.FileName}" });
                }

                if (fileUpload.File == null)
                {
                    return BadRequest(new { Message = "File is required for each upload" });
                }
            }

            var result = await _fileUploadService.UploadMultipleFilesAsync(request);
            
            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading multiple files");
            return StatusCode(500, new { Message = ex.Message });
        }
    }

    [HttpGet("processed/{mutualFundId}")]
    public async Task<IActionResult> GetProcessedFiles(int mutualFundId)
    {
        try
        {
            if (mutualFundId <= 0)
            {
                return BadRequest(new { Message = "Valid mutual fund ID is required" });
            }

            var result = await _fileUploadService.GetProcessedFilesAsync(mutualFundId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting processed files for mutual fund {MutualFundId}", mutualFundId);
            return StatusCode(500, new { Message = ex.Message });
        }
    }

    [HttpPost("validate")]
    public async Task<IActionResult> ValidateFile(IFormFile file)
    {
        try
        {
            if (file == null)
            {
                return BadRequest(new { Message = "File is required" });
            }

            var isValid = await _fileUploadService.ValidateFileAsync(file);
            return Ok(new { IsValid = isValid, FileName = file.FileName });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating file {FileName}", file?.FileName);
            return StatusCode(500, new { Message = ex.Message });
        }
    }
}
