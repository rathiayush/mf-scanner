using MutualFundScanner.Application.DTOs;
using Microsoft.AspNetCore.Http;

namespace MutualFundScanner.Application.Interfaces;

public interface IPdfProcessingService
{
    /// <summary>
    /// Process the uploaded PDF file: detect fund names, extract tables, and generate Excel reports.
    /// </summary>
    /// <param name="request">PDF processing request containing file, output folder, and other settings.</param>
    /// <returns>Processing result including detected funds, extracted tables, and created Excel files.</returns>
    Task<PdfProcessingResponseDto> ProcessPdfFileAsync(PdfProcessingRequestDto request);

    /// <summary>
    /// Extract text from all pages of the PDF.
    /// </summary>
    /// <param name="file">PDF file uploaded by user.</param>
    /// <returns>List of page data including text, lines, and table detection flags.</returns>
    Task<List<PdfPageData>> ExtractTextFromPdfAsync(IFormFile file);

    /// <summary>
    /// Detect all known fund names present on a single PDF page.
    /// </summary>
    /// <param name="page">Page data with text content.</param>
    /// <param name="knownFundNames">List of known fund names to detect.</param>
    /// <returns>List of detected fund results with confidence scores.</returns>
    Task<List<FundDetectionResult>> DetectFundNamesOnPageAsync(PdfPageData page, List<string> knownFundNames);

    /// <summary>
    /// Extract issuer/instrument tables from a given page.
    /// </summary>
    /// <param name="page">PDF page data.</param>
    /// <returns>List of extracted tables for that page.</returns>
    Task<List<PdfExtractedTableData>> ExtractIssuerInstrumentTableAsync(PdfPageData page);

    /// <summary>
    /// Generate Excel reports for the detected funds and extracted table data.
    /// </summary>
    /// <param name="fundNames">List of detected fund names.</param>
    /// <param name="tableData">List of extracted tables from PDF.</param>
    /// <param name="outputFolder">Folder path to save Excel reports.</param>
    /// <returns>List of created Excel files with metadata.</returns>
    Task<List<CreatedExcelFile>> GenerateExcelReportsAsync(List<string> fundNames, List<PdfExtractedTableData> tableData, string outputFolder);

    /// <summary>
    /// Validate whether the uploaded file is a valid PDF.
    /// </summary>
    /// <param name="file">Uploaded file.</param>
    /// <returns>True if file is valid PDF; otherwise, false.</returns>
    Task<bool> ValidatePdfFileAsync(IFormFile file);
}
