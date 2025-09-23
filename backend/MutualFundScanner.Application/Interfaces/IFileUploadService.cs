using MutualFundScanner.Application.DTOs;
using Microsoft.AspNetCore.Http;

namespace MutualFundScanner.Application.Interfaces;

public interface IFileUploadService
{
    Task<FileUploadResponseDto> UploadFilesAsync(FileUploadRequestDto request);
    Task<FileUploadResponseDto> UploadMultipleFilesAsync(MultipleFileUploadRequestDto request);
    Task<List<ProcessedFileDto>> GetProcessedFilesAsync(int mutualFundId);
    Task<bool> ValidateFileAsync(IFormFile file);


    Task<ProcessedFileDto> ProcessSingleFileAsync(IFormFile file, int mutualFundId, DateTime? reportDate = null);
    Task<ProcessedFileDto> ProcessSingleFileAsync(IFormFile file, int mutualFundId, DateTime? reportDate, int? sheetIndex);
}
