using MutualFundScanner.Application.DTOs;

namespace MutualFundScanner.Application.Interfaces;

public interface IUploadV2Service
{
    Task<UploadV2ResponseDto> ProcessUploadV2Async(UploadV2RequestDto request);
}
