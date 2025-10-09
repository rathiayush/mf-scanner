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
        
        // Create TopSharesRequestDto for the new method signature
        var topSharesRequest = new TopSharesRequestDto
        {
            MutualFundIds = request.MutualFundIds,
            FromDate = request.FromDate,
            ToDate = request.ToDate,
            Page = 1,
            PageSize = 15, // Default top shares count
            SortBy = "TotalWeight",
            SortDirection = "desc"
        };
        var topSharesResult = await GetTopSharesAsync(topSharesRequest);
        
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
            TopShares = topSharesResult, // Return full paged result
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
            ChangeTypeFilters = request.ChangeTypeFilters,
            ChangeTypeMatchMode = request.ChangeTypeMatchMode,
            SortBy = request.SortBy,
            SortDirection = request.SortDirection
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

        // Use database-level sorting and pagination for better performance
        return await GetHoldingChangesWithDatabaseSortingAsync(resolvedRequest, fundIds);
    }

    /// <summary>
    /// Gets holding changes with database-level sorting and pagination for optimal performance
    /// </summary>
    private async Task<PagedResult<HoldingChangeDto>> GetHoldingChangesWithDatabaseSortingAsync(
        AnalyticsRequestDto request, 
        List<int> fundIds)
    {
        // Get individual snapshots for from and to dates in a single optimized call
        var (fromSnapshots, toSnapshots) = await _snapshotFetchingHelper.GetFromAndToSnapshots(fundIds, request.FromDate, request.ToDate);
        
        // Filter fund IDs to only include those present in both snapshots AND have different snapshot dates
        // This ensures funds have data for both time periods, not just one snapshot in the range
        var fromFundIds = fromSnapshots.Select(s => s.MutualFundId).ToHashSet();
        var toFundIds = toSnapshots.Select(s => s.MutualFundId).ToHashSet();
        var commonFundIds = fromFundIds.Intersect(toFundIds).ToList();
        
        // Additional validation: Ensure from and to snapshots are actually different dates
        // This prevents including funds that only have one snapshot in the entire range
        var validFundIds = new List<int>();
        foreach (var fundId in commonFundIds)
        {
            var fromSnapshot = fromSnapshots.First(s => s.MutualFundId == fundId);
            var toSnapshot = toSnapshots.First(s => s.MutualFundId == fundId);
            
            // Only include if snapshots are from different dates (indicating data in multiple periods)
            if (fromSnapshot.SnapshotDate.Date != toSnapshot.SnapshotDate.Date)
            {
                validFundIds.Add(fundId);
            }
            else
            {
                _logger.LogWarning("Excluding fund {FundId} - only has single snapshot on {Date} within range {FromDate} to {ToDate}",
                    fundId, fromSnapshot.SnapshotDate.Date, request.FromDate, request.ToDate);
            }
        }
        
        // Filter snapshots to only include valid fund IDs
        fromSnapshots = fromSnapshots.Where(s => validFundIds.Contains(s.MutualFundId)).ToList();
        toSnapshots = toSnapshots.Where(s => validFundIds.Contains(s.MutualFundId)).ToList();
        
        _logger.LogInformation("Filtered to {ValidCount} funds with data in both time periods (from {CommonCount} funds with snapshots in range)",
            validFundIds.Count, commonFundIds.Count);
        
        // Get fund names for the calculation
        var fundNames = await _snapshotFetchingHelper.GetMutualFundNames(validFundIds);

        // Calculate holding changes from individual snapshots
        var changes = _analyticsCalculationHelper.CalculateHoldingChangesFromSnapshots(
            fromSnapshots, 
            toSnapshots, 
            fundNames, 
            request.FromDate, 
            request.ToDate);

        // Group changes by ISIN
        var groupedChanges = _analyticsCalculationHelper.GroupChangesByISIN(changes, request.FromDate, request.ToDate);

        // Apply database-level sorting by querying holdings with sorting applied
        var sortedAndFilteredChanges = await ApplyDatabaseLevelSortingAsync(
            groupedChanges, 
            request.ChangeTypeFilters, 
            request.ChangeTypeMatchMode,
            request.SectorFilters,
            request.IndustryFilters,
            request.WeightChangeMin,
            request.WeightChangeMax,
            request.QuantityChangeMin,
            request.QuantityChangeMax,
            request.ValueChangeMin,
            request.ValueChangeMax,
            request.MinFundsCount,
            request.SortBy, 
            request.SortDirection);

        // Apply pagination to the sorted results
        var totalCount = sortedAndFilteredChanges.Count;
        var pagedChanges = sortedAndFilteredChanges
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        return new PagedResult<HoldingChangeDto>
        {
            Data = pagedChanges,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }

    /// <summary>
    /// Applies database-level sorting by re-querying holdings with proper ordering
    /// Uses hybrid approach: database sorting for direct fields, in-memory for calculated fields
    /// </summary>
    private async Task<List<HoldingChangeDto>> ApplyDatabaseLevelSortingAsync(
        List<HoldingChangeDto> changes,
        List<string>? changeTypeFilters,
        string? changeTypeMatchMode,
        List<string>? sectorFilters,
        List<string>? industryFilters,
        decimal? weightChangeMin,
        decimal? weightChangeMax,
        decimal? quantityChangeMin,
        decimal? quantityChangeMax,
        decimal? valueChangeMin,
        decimal? valueChangeMax,
        int? minFundsCount,
        string? sortBy,
        string? sortDirection)
    {
        // Apply filtering first
        var filteredChanges = changes;

        // Apply change type filtering (existing logic)
        if (changeTypeFilters != null && changeTypeFilters.Any())
        {
            var set = changeTypeFilters.ToHashSet(StringComparer.OrdinalIgnoreCase);
            _logger.LogInformation("Applying change type filters: {Filters}", string.Join(",", changeTypeFilters));
            var mode = (changeTypeMatchMode ?? "any").ToLower();
            _logger.LogInformation("ChangeTypeMatchMode: {Mode}", mode);
            if (mode == "only")
            {
                filteredChanges = filteredChanges.Where(c =>
                {
                    // Build the constituent type set for this change
                    HashSet<string> types;
                    if (c.ChangeType == "Mixed" && c.FundChangeDetails != null && c.FundChangeDetails.Any())
                    {
                        types = c.FundChangeDetails.Select(fd => fd.ChangeType).ToHashSet(StringComparer.OrdinalIgnoreCase);
                    }
                    else
                    {
                        types = new HashSet<string>(new[] { c.ChangeType }, StringComparer.OrdinalIgnoreCase);
                    }

                    // Exact set equality: include only if constituent types exactly match selected set (no extras)
                    return types.SetEquals(set);
                }).ToList();
                _logger.LogInformation("Only mode filtered count: {Count}", filteredChanges.Count);
            }
            else
            {
                // 'any' mode (OR): include if direct match or any constituent in Mixed matches
                filteredChanges = filteredChanges.Where(c =>
                {
                    if (set.Contains(c.ChangeType)) return true;
                    if (c.ChangeType == "Mixed" && c.FundChangeDetails != null)
                    {
                        return c.FundChangeDetails.Any(fd => set.Contains(fd.ChangeType));
                    }
                    return false;
                }).ToList();
                _logger.LogInformation("Any mode filtered count: {Count}", filteredChanges.Count);
            }
        }

        // Apply sector filtering
        if (sectorFilters != null && sectorFilters.Any())
        {
            var set = sectorFilters.ToHashSet(StringComparer.OrdinalIgnoreCase);
            filteredChanges = filteredChanges.Where(c => set.Contains(c.Sector)).ToList();
            _logger.LogInformation("Applied sector filters: {Filters}, remaining count: {Count}", string.Join(",", sectorFilters), filteredChanges.Count);
        }

        // Apply industry filtering
        if (industryFilters != null && industryFilters.Any())
        {
            var set = industryFilters.ToHashSet(StringComparer.OrdinalIgnoreCase);
            filteredChanges = filteredChanges.Where(c => set.Contains(c.Industry)).ToList();
            _logger.LogInformation("Applied industry filters: {Filters}, remaining count: {Count}", string.Join(",", industryFilters), filteredChanges.Count);
        }

        // Apply weight change range filtering
        if (weightChangeMin.HasValue)
        {
            filteredChanges = filteredChanges.Where(c => c.WeightChange >= weightChangeMin.Value).ToList();
            _logger.LogInformation("Applied weight change min filter: {Min}, remaining count: {Count}", weightChangeMin.Value, filteredChanges.Count);
        }
        if (weightChangeMax.HasValue)
        {
            filteredChanges = filteredChanges.Where(c => c.WeightChange <= weightChangeMax.Value).ToList();
            _logger.LogInformation("Applied weight change max filter: {Max}, remaining count: {Count}", weightChangeMax.Value, filteredChanges.Count);
        }

        // Apply quantity change range filtering
        if (quantityChangeMin.HasValue)
        {
            filteredChanges = filteredChanges.Where(c => c.QuantityChange >= quantityChangeMin.Value).ToList();
            _logger.LogInformation("Applied quantity change min filter: {Min}, remaining count: {Count}", quantityChangeMin.Value, filteredChanges.Count);
        }
        if (quantityChangeMax.HasValue)
        {
            filteredChanges = filteredChanges.Where(c => c.QuantityChange <= quantityChangeMax.Value).ToList();
            _logger.LogInformation("Applied quantity change max filter: {Max}, remaining count: {Count}", quantityChangeMax.Value, filteredChanges.Count);
        }

        // Apply value change range filtering
        if (valueChangeMin.HasValue)
        {
            filteredChanges = filteredChanges.Where(c => c.ValueChange >= valueChangeMin.Value).ToList();
            _logger.LogInformation("Applied value change min filter: {Min}, remaining count: {Count}", valueChangeMin.Value, filteredChanges.Count);
        }
        if (valueChangeMax.HasValue)
        {
            filteredChanges = filteredChanges.Where(c => c.ValueChange <= valueChangeMax.Value).ToList();
            _logger.LogInformation("Applied value change max filter: {Max}, remaining count: {Count}", valueChangeMax.Value, filteredChanges.Count);
        }

        // Apply minimum funds count filtering
        if (minFundsCount.HasValue)
        {
            filteredChanges = filteredChanges.Where(c => c.MutualFundNames.Count >= minFundsCount.Value).ToList();
            _logger.LogInformation("Applied min funds count filter: {Min}, remaining count: {Count}", minFundsCount.Value, filteredChanges.Count);
        }

        // Determine if we can use database sorting or need in-memory sorting
        var calculatedFields = new[] { "weightchange", "mutualfundscount", "previousweight", "currentweight", "changetype" };
        var useInMemorySorting = string.IsNullOrEmpty(sortBy) || calculatedFields.Contains(sortBy.ToLower());

        if (useInMemorySorting)
        {
            // Use in-memory sorting for calculated fields
            _logger.LogInformation("Using in-memory sorting for calculated field: {SortBy}", sortBy ?? "default");
            return ApplyInMemorySorting(filteredChanges, sortBy, sortDirection);
        }

        // Use database-level sorting for direct fields
        if (filteredChanges.Any())
        {
            _logger.LogInformation("Using database-level sorting for field: {SortBy}", sortBy);
            var isins = filteredChanges.Select(c => c.ISIN).ToList();
            
            // Query holdings from database with sorting applied
            var sortedHoldings = await GetSortedHoldingsFromDatabase(isins, sortBy, sortDirection);
            
            // Reorder the changes based on database sorting
            var orderedChanges = new List<HoldingChangeDto>();
            foreach (var holding in sortedHoldings)
            {
                var matchingChange = filteredChanges.FirstOrDefault(c => c.ISIN == holding.ISIN);
                if (matchingChange != null)
                {
                    orderedChanges.Add(matchingChange);
                }
            }
            
            // Add any remaining changes that weren't found in database (shouldn't happen normally)
            var remainingChanges = filteredChanges.Where(c => !orderedChanges.Any(oc => oc.ISIN == c.ISIN)).ToList();
            orderedChanges.AddRange(remainingChanges);
            
            return orderedChanges;
        }

        return filteredChanges;
    }

    /// <summary>
    /// Applies in-memory sorting for calculated fields that cannot be sorted at database level
    /// </summary>
    private List<HoldingChangeDto> ApplyInMemorySorting(
        List<HoldingChangeDto> changes, 
        string? sortBy, 
        string? sortDirection)
    {
        if (string.IsNullOrEmpty(sortBy))
        {
            // Default sorting by absolute weight change descending
            return changes.OrderByDescending(c => Math.Abs(c.WeightChange)).ToList();
        }

        var isAscending = sortDirection?.ToLower() == "asc";
        
        return sortBy.ToLower() switch
        {
            "previousweight" => isAscending 
                ? changes.OrderBy(c => c.PreviousWeight ?? 0).ToList()
                : changes.OrderByDescending(c => c.PreviousWeight ?? 0).ToList(),
            
            "currentweight" => isAscending 
                ? changes.OrderBy(c => c.CurrentWeight).ToList()
                : changes.OrderByDescending(c => c.CurrentWeight).ToList(),
            
            "weightchange" => isAscending 
                ? changes.OrderBy(c => c.WeightChange).ToList()
                : changes.OrderByDescending(c => c.WeightChange).ToList(),
            
            "changetype" => isAscending 
                ? changes.OrderBy(c => c.ChangeType).ToList()
                : changes.OrderByDescending(c => c.ChangeType).ToList(),
            
            "mutualfundscount" => isAscending 
                ? changes.OrderBy(c => c.MutualFundNames.Count).ToList()
                : changes.OrderByDescending(c => c.MutualFundNames.Count).ToList(),
            
            _ => changes.OrderByDescending(c => Math.Abs(c.WeightChange)).ToList() // Default fallback
        };
    }

    /// <summary>
    /// Gets holdings from database with proper sorting applied
    /// </summary>
    private async Task<List<PortfolioHolding>> GetSortedHoldingsFromDatabase(
        List<string> isins, 
        string? sortBy, 
        string? sortDirection)
    {
        var query = _context.PortfolioHoldings
            .Where(h => isins.Contains(h.ISIN ?? ""));

        var isAscending = sortDirection?.ToLower() == "asc";

        // Apply database-level sorting
        query = (sortBy?.ToLower()) switch
        {
            "securityname" => isAscending 
                ? query.OrderBy(h => h.SecurityName)
                : query.OrderByDescending(h => h.SecurityName),
            
            "sector" => isAscending 
                ? query.OrderBy(h => h.Sector)
                : query.OrderByDescending(h => h.Sector),
            
            "industry" => isAscending 
                ? query.OrderBy(h => h.Industry)
                : query.OrderByDescending(h => h.Industry),
            
            "isin" => isAscending 
                ? query.OrderBy(h => h.ISIN)
                : query.OrderByDescending(h => h.ISIN),
            
            _ => query.OrderByDescending(h => h.Weight) // Default sorting
        };

        return await query
            .GroupBy(h => h.ISIN)
            .Select(g => g.First()) // Take first occurrence of each ISIN
            .ToListAsync();
    }

    public async Task<PagedResult<TopShareDto>> GetTopSharesAsync(TopSharesRequestDto request)
    {
        // Resolve "All Funds" request if needed
        var fundIds = await _snapshotFetchingHelper.ResolveAllFunds(request.MutualFundIds, _logger);

        var snapshots = await _snapshotFetchingHelper.GetLatestSnapshotsBeforeDate(fundIds, request.ToDate);
        var fundNames = await _snapshotFetchingHelper.GetMutualFundNames(fundIds);

        // Create all top shares without limit first
        var allTopShares = _analyticsCalculationHelper.CreateTopSharesFromSnapshots(snapshots, fundNames);

        // Apply database-level sorting for direct fields, in-memory for calculated fields
        var sortedShares = await ApplyTopSharesSortingAsync(allTopShares, request.SortBy, request.SortDirection);

        // Apply pagination
        var totalCount = sortedShares.Count;
        var pagedShares = sortedShares
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        return new PagedResult<TopShareDto>
        {
            Data = pagedShares,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }

    /// <summary>
    /// Applies sorting to top shares using hybrid database/in-memory approach
    /// </summary>
    private async Task<List<TopShareDto>> ApplyTopSharesSortingAsync(
        List<TopShareDto> topShares,
        string? sortBy,
        string? sortDirection)
    {
        // Determine if we can use database sorting or need in-memory sorting
        var calculatedFields = new[] { "totalweight", "holdingfundscount" };
        var useInMemorySorting = string.IsNullOrEmpty(sortBy) || calculatedFields.Contains(sortBy.ToLower());

        if (useInMemorySorting)
        {
            // Use in-memory sorting for calculated fields
            _logger.LogInformation("Using in-memory sorting for top shares field: {SortBy}", sortBy ?? "default");
            return ApplyTopSharesInMemorySorting(topShares, sortBy, sortDirection);
        }

        // Use database-level sorting for direct fields
        if (topShares.Any())
        {
            _logger.LogInformation("Using database-level sorting for top shares field: {SortBy}", sortBy);
            var isins = topShares.Select(ts => ts.ISIN).ToList();
            
            // Query holdings from database with sorting applied
            var sortedHoldings = await GetSortedHoldingsFromDatabase(isins, sortBy, sortDirection);
            
            // Reorder the top shares based on database sorting
            var orderedShares = new List<TopShareDto>();
            foreach (var holding in sortedHoldings)
            {
                var matchingShare = topShares.FirstOrDefault(ts => ts.ISIN == holding.ISIN);
                if (matchingShare != null)
                {
                    orderedShares.Add(matchingShare);
                }
            }
            
            // Add any remaining shares that weren't found in database
            var remainingShares = topShares.Where(ts => !orderedShares.Any(os => os.ISIN == ts.ISIN)).ToList();
            orderedShares.AddRange(remainingShares);
            
            return orderedShares;
        }

        return topShares;
    }

    /// <summary>
    /// Applies in-memory sorting for top shares calculated fields
    /// </summary>
    private List<TopShareDto> ApplyTopSharesInMemorySorting(
        List<TopShareDto> topShares,
        string? sortBy,
        string? sortDirection)
    {
        if (string.IsNullOrEmpty(sortBy))
        {
            // Default sorting by total weight descending
            return topShares.OrderByDescending(ts => ts.TotalWeight).ToList();
        }

        var isAscending = sortDirection?.ToLower() == "asc";
        
        return sortBy.ToLower() switch
        {
            "totalweight" => isAscending 
                ? topShares.OrderBy(ts => ts.TotalWeight).ToList()
                : topShares.OrderByDescending(ts => ts.TotalWeight).ToList(),
            
            "holdingfundscount" => isAscending 
                ? topShares.OrderBy(ts => ts.HoldingFundsCount).ToList()
                : topShares.OrderByDescending(ts => ts.HoldingFundsCount).ToList(),
            
            _ => topShares.OrderByDescending(ts => ts.TotalWeight).ToList() // Default fallback
        };
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
            
            // Apply database-level sorting to holdings
            var sortedHoldings = ApplyCurrentHoldingsSorting(holdings, request.SortBy, request.SortDirection);
            
            var holdingDtos = sortedHoldings.Select(h => new CurrentHoldingDto
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

            // Apply pagination
            var totalCount = holdingDtos.Count;
            var pagedHoldings = holdingDtos
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            var pagedResult = new PagedResult<CurrentHoldingDto>
            {
                Data = pagedHoldings,
                TotalCount = totalCount,
                Page = request.Page,
                PageSize = request.PageSize
            };

            return new CurrentHoldingsResponseDto
            {
                Success = true,
                Message = "Holdings retrieved successfully",
                MutualFundId = mutualFund.Id,
                MutualFundName = mutualFund.FundName,
                Date = portfolioFile.ReportDate,
                TotalValue = totalValue,
                Holdings = pagedResult,
                HoldingsCount = totalCount
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

    /// <summary>
    /// Applies sorting to current holdings
    /// </summary>
    private List<PortfolioHolding> ApplyCurrentHoldingsSorting(
        List<PortfolioHolding> holdings,
        string? sortBy,
        string? sortDirection)
    {
        if (string.IsNullOrEmpty(sortBy))
        {
            // Default sorting by market value descending
            return holdings.OrderByDescending(h => h.MarketValue).ToList();
        }

        var isAscending = sortDirection?.ToLower() == "asc";
        
        return sortBy.ToLower() switch
        {
            "symbol" => isAscending 
                ? holdings.OrderBy(h => h.Ticker ?? h.ISIN ?? "").ToList()
                : holdings.OrderByDescending(h => h.Ticker ?? h.ISIN ?? "").ToList(),
            
            "companyname" => isAscending 
                ? holdings.OrderBy(h => h.SecurityName ?? "").ToList()
                : holdings.OrderByDescending(h => h.SecurityName ?? "").ToList(),
            
            "quantity" => isAscending 
                ? holdings.OrderBy(h => h.Quantity).ToList()
                : holdings.OrderByDescending(h => h.Quantity).ToList(),
            
            "price" => isAscending 
                ? holdings.OrderBy(h => h.MarketValue / (h.Quantity > 0 ? h.Quantity : 1)).ToList()
                : holdings.OrderByDescending(h => h.MarketValue / (h.Quantity > 0 ? h.Quantity : 1)).ToList(),
            
            "value" => isAscending 
                ? holdings.OrderBy(h => h.MarketValue).ToList()
                : holdings.OrderByDescending(h => h.MarketValue).ToList(),
            
            "weight" => isAscending 
                ? holdings.OrderBy(h => h.Weight).ToList()
                : holdings.OrderByDescending(h => h.Weight).ToList(),
            
            "sector" => isAscending 
                ? holdings.OrderBy(h => h.Sector ?? "").ToList()
                : holdings.OrderByDescending(h => h.Sector ?? "").ToList(),
            
            "industry" => isAscending 
                ? holdings.OrderBy(h => h.Industry ?? "").ToList()
                : holdings.OrderByDescending(h => h.Industry ?? "").ToList(),
            
            _ => holdings.OrderByDescending(h => h.MarketValue).ToList() // Default fallback
        };
    }
}
