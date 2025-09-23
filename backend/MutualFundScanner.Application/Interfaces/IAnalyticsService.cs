using MutualFundScanner.Application.DTOs;

namespace MutualFundScanner.Application.Interfaces;

public interface IAnalyticsService
{
    Task<AnalyticsResponseDto> GetAnalyticsAsync(AnalyticsRequestDto request);
    Task<PagedResult<HoldingChangeDto>> GetHoldingChangesAsync(AnalyticsRequestDto request);
    Task<List<TopShareDto>> GetTopSharesAsync(List<int> mutualFundIds, DateTime fromDate, DateTime toDate, int topCount = 15);
    Task<AvailableDatesResponseDto> GetAvailableDatesAsync(List<int>? mutualFundIds = null);
    Task<AnalyticsSummaryDto> GetAnalyticsSummaryAsync(List<int> mutualFundIds, DateTime fromDate, DateTime toDate);
    Task<CurrentHoldingsResponseDto> GetCurrentHoldingsAsync(CurrentHoldingsRequestDto request);
    Task<bool> ValidateDatesForFundsAsync(List<int> mutualFundIds, DateTime fromDate, DateTime toDate);
}
