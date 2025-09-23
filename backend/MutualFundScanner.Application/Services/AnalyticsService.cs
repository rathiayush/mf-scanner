using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MutualFundScanner.Application.DTOs;
using MutualFundScanner.Application.Helpers;
using MutualFundScanner.Application.Interfaces;
using MutualFundScanner.Domain.Entities;
using MutualFundScanner.Domain.Enums;
using MutualFundScanner.Infrastructure.Data;

namespace MutualFundScanner.Application.Services;

public class AnalyticsService : IAnalyticsService
{
    private readonly MutualFundContext _context;
    private readonly ILogger<AnalyticsService> _logger;
    private readonly AnalyticsCalculationHelper _analyticsCalculationHelper;
    private readonly DataValidationHelper _dataValidationHelper;
    private readonly ISnapshotFetchingHelper _snapshotFetchingHelper;

    public AnalyticsService(
        MutualFundContext context, 
        ILogger<AnalyticsService> logger,
        AnalyticsCalculationHelper analyticsCalculationHelper,
        DataValidationHelper dataValidationHelper,
        ISnapshotFetchingHelper snapshotFetchingHelper)
    {
        _context = context;
        _logger = logger;
        _analyticsCalculationHelper = analyticsCalculationHelper;
        _dataValidationHelper = dataValidationHelper;
        _snapshotFetchingHelper = snapshotFetchingHelper;
    }

    public async Task<AnalyticsResponseDto> GetAnalyticsAsync(AnalyticsRequestDto request)
    {
        var holdingChanges = await GetHoldingChangesAsync(request);
        var topShares = await GetTopSharesAsync(request.MutualFundIds, request.FromDate, request.ToDate);
        var summary = await GetAnalyticsSummaryAsync(request.MutualFundIds, request.FromDate, request.ToDate);

        // If original request had empty fund IDs, get all fund IDs for response
        var responseFundIds = request.MutualFundIds;
        if (responseFundIds == null || !responseFundIds.Any())
        {
            responseFundIds = await _context.MutualFunds.Select(mf => mf.Id).ToListAsync();
        }

        return new AnalyticsResponseDto
        {
            MutualFundIds = responseFundIds,
            FromDate = request.FromDate,
            ToDate = request.ToDate,
            AddedRemovedHoldings = holdingChanges,
            TopShares = topShares,
            Summary = summary
        };
    }

    public async Task<PagedResult<HoldingChangeDto>> GetHoldingChangesAsync(AnalyticsRequestDto request)
    {
        // Resolve "All Funds" request if needed
        var fundIds = await _snapshotFetchingHelper.ResolveAllFunds(request.MutualFundIds, _logger);

        // Create a new request with the resolved fund IDs
        var resolvedRequest = new AnalyticsRequestDto
        {
            MutualFundIds = fundIds,
            FromDate = request.FromDate,
            ToDate = request.ToDate,
            Page = request.Page,
            PageSize = request.PageSize,
            ChangeTypeFilter = request.ChangeTypeFilter
        };

        // Validate request
        var (isValid, errorMessage) = _dataValidationHelper.ValidateAnalyticsRequest(resolvedRequest);
        if (!isValid)
        {
            throw new ArgumentException(errorMessage);
        }

        // Validate dates for funds
        var (datesValid, datesErrorMessage) = await _dataValidationHelper.ValidateDatesForFundsAsync(fundIds, request.FromDate, request.ToDate);
        if (!datesValid)
        {
            throw new ArgumentException(datesErrorMessage);
        }

        // Get individual snapshots for from and to dates in a single optimized call
        var (fromSnapshots, toSnapshots) = await _snapshotFetchingHelper.GetFromAndToSnapshots(fundIds, request.FromDate, request.ToDate);
        
        // Filter fund IDs to only include those present in both snapshots
        var fromFundIds = fromSnapshots.Select(s => s.MutualFundId).ToHashSet();
        var toFundIds = toSnapshots.Select(s => s.MutualFundId).ToHashSet();
        var commonFundIds = fromFundIds.Intersect(toFundIds).ToList();
        
        // Filter snapshots to only include common fund IDs
        fromSnapshots = fromSnapshots.Where(s => commonFundIds.Contains(s.MutualFundId)).ToList();
        toSnapshots = toSnapshots.Where(s => commonFundIds.Contains(s.MutualFundId)).ToList();
        
        // Get fund names for the calculation
        var fundNames = await _snapshotFetchingHelper.GetMutualFundNames(commonFundIds);

        // Calculate holding changes from individual snapshots
        var changes = _analyticsCalculationHelper.CalculateHoldingChangesFromSnapshots(
            fromSnapshots, 
            toSnapshots, 
            fundNames, 
            request.FromDate, 
            request.ToDate);

        // Group changes by ISIN
        var groupedChanges = _analyticsCalculationHelper.GroupChangesByISIN(changes);

        // Apply filtering and pagination
        return _analyticsCalculationHelper.ApplyFilteringAndPagination(
            groupedChanges, 
            request.ChangeTypeFilter, 
            request.Page, 
            request.PageSize);
    }

    public async Task<List<TopShareDto>> GetTopSharesAsync(List<int> mutualFundIds, DateTime fromDate, DateTime toDate, int topCount = 10)
    {
        // Resolve "All Funds" request if needed
        var fundIds = await _snapshotFetchingHelper.ResolveAllFunds(mutualFundIds, _logger);

        var snapshots = await _snapshotFetchingHelper.GetLatestSnapshotsBeforeDate(fundIds, toDate);
        var fundNames = await _snapshotFetchingHelper.GetMutualFundNames(fundIds);

        return _analyticsCalculationHelper.CreateTopSharesFromSnapshots(snapshots, fundNames, topCount);
    }

    public async Task<bool> ValidateDatesForFundsAsync(List<int> mutualFundIds, DateTime fromDate, DateTime toDate)
    {
        // Resolve "All Funds" request if needed
        var fundIds = await _snapshotFetchingHelper.ResolveAllFunds(mutualFundIds, _logger);

        var (isValid, _) = await _dataValidationHelper.ValidateDatesForFundsAsync(fundIds, fromDate, toDate);
        return isValid;
    }

    public async Task<AvailableDatesResponseDto> GetAvailableDatesAsync(List<int>? mutualFundIds = null)
    {
        return await _dataValidationHelper.GetAvailableDatesAsync(mutualFundIds);
    }

    public async Task<AnalyticsSummaryDto> GetAnalyticsSummaryAsync(List<int> mutualFundIds, DateTime fromDate, DateTime toDate)
    {
        // Resolve "All Funds" request if needed
        var fundIds = await _snapshotFetchingHelper.ResolveAllFunds(mutualFundIds, _logger);

        var holdingChanges = await GetHoldingChangesAsync(new AnalyticsRequestDto
        {
            MutualFundIds = fundIds,
            FromDate = fromDate,
            ToDate = toDate,
            Page = 1,
            PageSize = int.MaxValue
        });

        return _analyticsCalculationHelper.CreateAnalyticsSummary(holdingChanges.Data);
    }


    /// <summary>
    /// Fetches snapshots for specified mutual funds within a date range
    /// Delegates to SnapshotFetchingHelper for consistent behavior
    /// </summary>
    public async Task<List<PortfolioSnapshot>> GetSnapshotsInDateRange(
        List<int> mutualFundIds, 
        DateTime fromDate, 
        DateTime toDate)
    {
        return await _snapshotFetchingHelper.GetSnapshotsInDateRange(mutualFundIds, fromDate, toDate);
    }

    public async Task<CurrentHoldingsResponseDto> GetCurrentHoldingsAsync(CurrentHoldingsRequestDto request)
    {
        try
        {
            // Validate request
            var (isValid, errorMessage) = await _dataValidationHelper.ValidateCurrentHoldingsRequest(request);
            if (!isValid)
            {
                return new CurrentHoldingsResponseDto
                {
                    Success = false,
                    Message = errorMessage
                };
            }

            // Get mutual fund details
            var mutualFund = await _context.MutualFunds.FindAsync(request.MutualFundId);
            if (mutualFund == null)
            {
                return new CurrentHoldingsResponseDto
                {
                    Success = false,
                    Message = $"Mutual fund with ID {request.MutualFundId} not found"
                };
            }

            // Use date range from UI or default range if not provided
            var dateRangeStart = request.DateRangeStart ?? request.TargetDate.AddDays(-30);
            var dateRangeEnd = request.DateRangeEnd ?? request.TargetDate.AddDays(30);
            
            // First try exact date match within the date range
            var exactPortfolioFile = await _context.PortfolioFiles
                .Where(pf => pf.MutualFundId == request.MutualFundId && 
                           pf.Status == ProcessingStatus.Completed &&
                           pf.ReportDate.Date == request.TargetDate.Date &&
                           pf.ReportDate.Date >= dateRangeStart.Date && 
                           pf.ReportDate.Date <= dateRangeEnd.Date)
                .FirstOrDefaultAsync();

            PortfolioFile? portfolioFile = exactPortfolioFile;
            
            if (portfolioFile == null)
            {
                // If no exact match, find files within range and get the closest one
                var candidateFiles = await _context.PortfolioFiles
                    .Where(pf => pf.MutualFundId == request.MutualFundId && 
                               pf.Status == ProcessingStatus.Completed &&
                               pf.ReportDate.Date >= dateRangeStart.Date && 
                               pf.ReportDate.Date <= dateRangeEnd.Date)
                    .ToListAsync();

                // Find the closest one by calculating date difference
                portfolioFile = candidateFiles
                    .OrderBy(pf => Math.Abs((pf.ReportDate.Date - request.TargetDate.Date).TotalDays))
                    .FirstOrDefault();
            }

            if (portfolioFile == null)
            {
                return new CurrentHoldingsResponseDto
                {
                    Success = false,
                    Message = $"No portfolio data found for mutual fund {mutualFund.FundName} on or before {request.TargetDate:yyyy-MM-dd}"
                };
            }

            // Get holdings for the portfolio file
            var holdings = await _context.PortfolioHoldings
                .Where(ph => ph.PortfolioFileId == portfolioFile.Id)
                .OrderByDescending(ph => ph.MarketValue)
                .ToListAsync();

            var totalValue = holdings.Sum(h => h.MarketValue);
            
            var holdingDtos = holdings.Select(h => new CurrentHoldingDto
            {
                Id = (int)h.Id,
                Symbol = h.Ticker ?? h.ISIN ?? "Unknown",
                CompanyName = h.SecurityName ?? "Unknown",
                Quantity = h.Quantity,
                Price = h.MarketValue / (h.Quantity > 0 ? h.Quantity : 1),
                Value = h.MarketValue,
                Weight = h.Weight,
                Sector = h.Sector,
                Industry = h.Industry
            }).ToList();

            return new CurrentHoldingsResponseDto
            {
                Success = true,
                Message = "Holdings retrieved successfully",
                MutualFundId = mutualFund.Id,
                MutualFundName = mutualFund.FundName,
                Date = portfolioFile.ReportDate,
                TotalValue = totalValue,
                Holdings = holdingDtos,
                HoldingsCount = holdingDtos.Count()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current holdings for mutual fund {MutualFundId} on date {Date}", 
                request.MutualFundId, request.TargetDate);
            
            return new CurrentHoldingsResponseDto
            {
                Success = false,
                Message = "An error occurred while retrieving holdings"
            };
        }
    }

}
