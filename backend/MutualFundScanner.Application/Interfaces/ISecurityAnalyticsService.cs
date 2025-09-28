using MutualFundScanner.Application.DTOs;

namespace MutualFundScanner.Application.Interfaces;

public interface ISecurityAnalyticsService
{
    Task<AvailableSecuritiesResponseDto> GetAvailableSecuritiesAsync(string? securityNameSearch = null);
    Task<AvailableSecuritiesResponseDto> GetSecuritiesByIsinAsync(string? isinSearch = null);
    Task<SecurityAnalyticsResponseDto> GetSecurityAnalyticsAsync(SecurityAnalyticsRequestDto request);
}
