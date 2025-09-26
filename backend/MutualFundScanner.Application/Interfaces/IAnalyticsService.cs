using MutualFundScanner.Application.DTOs;

namespace MutualFundScanner.Application.Interfaces;

public interface IAnalyticsService
{
    Task<AnalyticsResponseDto> GetAnalyticsAsync(AnalyticsRequestDto request);
    Task<PagedResult<HoldingChangeDto>> GetHoldingChangesAsync(AnalyticsRequestDto request);
    Task<PagedResult<TopShareDto>> GetTopSharesAsync(TopSharesRequestDto request);
    Task<AvailableDatesResponseDto> GetAvailableDatesAsync(List<int>? mutualFundIds = null);
    Task<AnalyticsSummaryDto> GetAnalyticsSummaryAsync(List<int> mutualFundIds, DateTime fromDate, DateTime toDate);
    Task<CurrentHoldingsResponseDto> GetCurrentHoldingsAsync(CurrentHoldingsRequestDto request);
    Task<bool> ValidateDatesForFundsAsync(List<int> mutualFundIds, DateTime fromDate, DateTime toDate);
}
