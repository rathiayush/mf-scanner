using MutualFundScanner.Application.DTOs;

namespace MutualFundScanner.Application.Interfaces;

public interface IUploadV2Service
{
    Task<UploadV2ResponseDto> ProcessUploadV2Async(UploadV2RequestDto request);
    Task<UploadV2ResponseDto> ProcessUploadV2StreamlinedAsync(UploadV2RequestDto request);
}
