using Microsoft.AspNetCore.Mvc;
using MutualFundScanner.Application.DTOs;
using MutualFundScanner.Application.Interfaces;

namespace MutualFundScanner.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UploadV2Controller : ControllerBase
{
    private readonly IUploadV2Service _uploadV2Service;
    private readonly IPdfProcessingService _pdfProcessingService;
    private readonly ILogger<UploadV2Controller> _logger;

    public UploadV2Controller(IUploadV2Service uploadV2Service, IPdfProcessingService pdfProcessingService, ILogger<UploadV2Controller> logger)
    {
        _uploadV2Service = uploadV2Service;
        _pdfProcessingService = pdfProcessingService;
        _logger = logger;
    }

    [HttpPost("upload")]
    public async Task<IActionResult> UploadFiles([FromForm] UploadV2RequestDto request)
    {
        try
        {
            if (request.Files == null || !request.Files.Any())
            {
                return BadRequest(new { Message = "No files provided" });
            }

            if (request.ReportDate == default)
            {
                return BadRequest(new { Message = "Valid report date is required" });
            }

            // Validate file types
            var allowedExtensions = new[] { ".xlsx", ".xls" };
            foreach (var file in request.Files)
            {
                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(extension))
                {
                    return BadRequest(new { Message = $"File {file.FileName} has unsupported format. Only .xlsx and .xls files are allowed." });
                }
            }

            var result = await _uploadV2Service.ProcessUploadV2Async(request);
            
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
            _logger.LogError(ex, "Error in V2 upload endpoint");
            return StatusCode(500, new { Message = "Internal server error occurred during upload" });
        }
    }

    [HttpPost("process-pdf")]
    public async Task<IActionResult> ProcessPdf([FromForm] PdfProcessingRequestDto request)
    {
        try
        {
            if (request.File == null)
            {
                return BadRequest(new { Message = "No PDF file provided" });
            }

            // Validate file type
            var extension = Path.GetExtension(request.File.FileName).ToLowerInvariant();
            if (extension != ".pdf")
            {
                return BadRequest(new { Message = $"File {request.File.FileName} has unsupported format. Only PDF files are allowed." });
            }

            var result = await _pdfProcessingService.ProcessPdfFileAsync(request);
            
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
            _logger.LogError(ex, "Error in PDF processing endpoint");
            return StatusCode(500, new { Message = "Internal server error occurred during PDF processing" });
        }
    }

    [HttpGet("detection-patterns")]
    public IActionResult GetDetectionPatterns()
    {
        try
        {
            var patterns = new
            {
                Message = "V2 Upload automatically detects mutual funds from file content",
                ScanArea = "First 50 rows across all columns",
                MatchingLogic = "Text matching against predefined mutual fund list",
                AutoCreation = "Creates new mutual fund if not found in database",
                SupportedFormats = new[] { ".xlsx", ".xls" }
            };

            return Ok(patterns);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting detection patterns");
            return StatusCode(500, new { Message = "Error retrieving detection patterns" });
        }
    }
}
