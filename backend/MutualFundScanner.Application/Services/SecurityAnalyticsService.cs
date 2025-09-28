using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MutualFundScanner.Application.DTOs;
using MutualFundScanner.Application.Interfaces;
using MutualFundScanner.Domain.Entities;
using MutualFundScanner.Infrastructure.Data;

namespace MutualFundScanner.Application.Services;

public class SecurityAnalyticsService : ISecurityAnalyticsService
{
    private readonly MutualFundContext _context;
    private readonly ILogger<SecurityAnalyticsService> _logger;

    public SecurityAnalyticsService(
        MutualFundContext context,
        ILogger<SecurityAnalyticsService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<AvailableSecuritiesResponseDto> GetAvailableSecuritiesAsync(string? securityNameSearch = null)
    {
        try
        {
            var query = _context.PortfolioHoldings.AsQueryable();

            // Apply security name search (partial match, case-insensitive)
            if (!string.IsNullOrWhiteSpace(securityNameSearch))
            {
                securityNameSearch = securityNameSearch.ToLower();
                query = query.Where(h =>
                    h.SecurityName != null && h.SecurityName.ToLower().Contains(securityNameSearch)
                );
            }

            var securities = await query
                .Where(h => !string.IsNullOrEmpty(h.ISIN) && !string.IsNullOrEmpty(h.SecurityName))
                .GroupBy(h => new { h.ISIN, h.SecurityName })
                .Select(g => new SecurityDropdownDto
                {
                    ISIN = g.Key.ISIN!,
                    SecurityName = g.Key.SecurityName!,
                    DisplayName = $"{g.Key.SecurityName} ({g.Key.ISIN})"
                })
                .OrderBy(s => s.SecurityName)
                .ToListAsync();

            return new AvailableSecuritiesResponseDto
            {
                Securities = securities
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting available securities with securityNameSearch: {SecurityNameSearch}", securityNameSearch);
            return new AvailableSecuritiesResponseDto
            {
                Securities = new List<SecurityDropdownDto>()
            };
        }
    }

    public async Task<AvailableSecuritiesResponseDto> GetSecuritiesByIsinAsync(string? isinSearch = null)
    {
        try
        {
            var query = _context.PortfolioHoldings.AsQueryable();

            // Apply ISIN search (prefix matching, case-insensitive)
            if (!string.IsNullOrWhiteSpace(isinSearch))
            {
                isinSearch = isinSearch.ToLower();
                query = query.Where(h =>
                    h.ISIN != null && h.ISIN.ToLower().StartsWith(isinSearch)
                );
            }

            var securities = await query
                .Where(h => !string.IsNullOrEmpty(h.ISIN) && !string.IsNullOrEmpty(h.SecurityName))
                .GroupBy(h => new { h.ISIN, h.SecurityName })
                .Select(g => new SecurityDropdownDto
                {
                    ISIN = g.Key.ISIN!,
                    SecurityName = g.Key.SecurityName!,
                    DisplayName = $"{g.Key.SecurityName} ({g.Key.ISIN})"
                })
                .OrderBy(s => s.SecurityName)
                .ToListAsync();

            return new AvailableSecuritiesResponseDto
            {
                Securities = securities
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting securities by ISIN: {IsinSearch}", isinSearch);
            return new AvailableSecuritiesResponseDto
            {
                Securities = new List<SecurityDropdownDto>()
            };
        }
    }
    public async Task<SecurityAnalyticsResponseDto> GetSecurityAnalyticsAsync(SecurityAnalyticsRequestDto request)
    {
        try
        {
            // Validate request
            if (string.IsNullOrEmpty(request.ISIN) && string.IsNullOrEmpty(request.SecurityName))
            {
                throw new ArgumentException("Either ISIN or SecurityName must be provided");
            }

            // Find the security
            var securityQuery = _context.PortfolioHoldings.AsQueryable();

            if (!string.IsNullOrEmpty(request.ISIN))
            {
                securityQuery = securityQuery.Where(h => h.ISIN == request.ISIN);
            }

            if (!string.IsNullOrEmpty(request.SecurityName))
            {
                securityQuery = securityQuery.Where(h => h.SecurityName == request.SecurityName);
            }

            var security = await securityQuery
                .Select(h => new { h.ISIN, h.SecurityName })
                .FirstOrDefaultAsync();

            if (security == null)
            {
                return new SecurityAnalyticsResponseDto
                {
                    ISIN = request.ISIN ?? "",
                    SecurityName = request.SecurityName ?? "",
                    FromDate = request.FromDate,
                    ToDate = request.ToDate,
                    MonthlyChanges = new List<SecurityMonthlyChangeDto>(),
                    TotalMutualFunds = 0,
                    Summary = new SecurityAnalyticsSummaryDto()
                };
            }

            // Get all holdings for this security within the date range
            var allHoldings = await _context.PortfolioHoldings
                .Where(h => h.ISIN == security.ISIN &&
                           _context.PortfolioFiles.Any(pf =>
                               pf.Id == h.PortfolioFileId &&
                               pf.ReportDate >= request.FromDate &&
                               pf.ReportDate <= request.ToDate))
                .Include(h => h.PortfolioFile)
                .ThenInclude(pf => pf.MutualFund)
                .OrderBy(h => h.PortfolioFile!.ReportDate)
                .ToListAsync();

            // Group by month and mutual fund to analyze changes
            var monthlyChanges = await AnalyzeMonthlyChangesAsync(allHoldings, request.FromDate, request.ToDate);

            // Apply sorting
            var sortedChanges = ApplySorting(monthlyChanges, request.SortBy, request.SortDirection);

            // Apply pagination
            var totalCount = sortedChanges.Count;
            var pagedChanges = sortedChanges
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            // Calculate summary
            var summary = CalculateSummary(monthlyChanges);

            return new SecurityAnalyticsResponseDto
            {
                ISIN = security.ISIN!,
                SecurityName = security.SecurityName!,
                FromDate = request.FromDate,
                ToDate = request.ToDate,
                MonthlyChanges = pagedChanges,
                TotalMutualFunds = summary.TotalMutualFundsInvolved,
                Summary = summary
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting security analytics for ISIN: {ISIN}, SecurityName: {SecurityName}",
                request.ISIN, request.SecurityName);

            return new SecurityAnalyticsResponseDto
            {
                ISIN = request.ISIN ?? "",
                SecurityName = request.SecurityName ?? "",
                FromDate = request.FromDate,
                ToDate = request.ToDate,
                MonthlyChanges = new List<SecurityMonthlyChangeDto>(),
                TotalMutualFunds = 0,
                Summary = new SecurityAnalyticsSummaryDto()
            };
        }
    }

    private async Task<List<SecurityMonthlyChangeDto>> AnalyzeMonthlyChangesAsync(
        List<PortfolioHolding> holdings,
        DateTime fromDate,
        DateTime toDate)
    {
        var monthlyChanges = new List<SecurityMonthlyChangeDto>();

        // Group holdings by mutual fund and month
        var fundMonthHoldings = holdings
            .GroupBy(h => new {
                MutualFundId = h.PortfolioFile!.MutualFundId,
                Year = h.PortfolioFile.ReportDate.Year,
                Month = h.PortfolioFile.ReportDate.Month
            })
            .ToDictionary(
                g => $"{g.Key.Year}-{g.Key.Month:D2}-{g.Key.MutualFundId}",
                g => g.OrderByDescending(h => h.PortfolioFile!.ReportDate).First() // Take latest holding in month
            );

        // Generate all months in the range
        var currentMonth = new DateTime(fromDate.Year, fromDate.Month, 1);
        var endMonth = new DateTime(toDate.Year, toDate.Month, 1);

        while (currentMonth <= endMonth)
        {
            var monthKey = $"{currentMonth.Year}-{currentMonth.Month:D2}";
            var fundChanges = new List<SecurityFundChangeDto>();

            // Get all fund-security combinations for this month
            var currentMonthHoldings = fundMonthHoldings.Where(fh => fh.Key.StartsWith(monthKey)).ToList();

            foreach (var fundHolding in currentMonthHoldings)
            {
                var currentHolding = fundHolding.Value;
                var prevMonth = currentMonth.AddMonths(-1);
                var previousMonthKey = $"{prevMonth.Year}-{prevMonth.Month:D2}-{currentHolding.PortfolioFile!.MutualFundId}";

                // Find previous month's holding for this fund
                var hasPreviousHolding = fundMonthHoldings.TryGetValue(previousMonthKey, out var previousHolding);

                if (!hasPreviousHolding)
                {
                    // First time holding this security
                    if (currentHolding.Weight > 0)
                    {
                        fundChanges.Add(new SecurityFundChangeDto
                        {
                            MutualFundId = currentHolding.PortfolioFile.MutualFundId,
                            MutualFundName = currentHolding.PortfolioFile.MutualFund!.FundName,
                            PreviousWeight = 0,
                            CurrentWeight = currentHolding.Weight,
                            WeightChange = currentHolding.Weight,
                            ChangeType = "Added"
                        });
                    }
                }
                else
                {
                    // Compare with previous month
                    var weightChange = currentHolding.Weight - previousHolding.Weight;
                    var changeType = DetermineChangeType(previousHolding.Weight, currentHolding.Weight);

                    fundChanges.Add(new SecurityFundChangeDto
                    {
                        MutualFundId = currentHolding.PortfolioFile.MutualFundId,
                        MutualFundName = currentHolding.PortfolioFile.MutualFund!.FundName,
                        PreviousWeight = previousHolding.Weight,
                        CurrentWeight = currentHolding.Weight,
                        WeightChange = weightChange,
                        ChangeType = changeType
                    });
                }
            }

            // Check for removed funds - funds that had this security last month but don't have it this month
            var previousMonth = currentMonth.AddMonths(-1);
            var previousMonthKeyPattern = $"{previousMonth.Year}-{previousMonth.Month:D2}";
            var removedFunds = fundMonthHoldings
                .Where(fh => fh.Key.StartsWith(previousMonthKeyPattern))
                .Where(fh => !currentMonthHoldings.Any(cmh => cmh.Key.EndsWith($"-{fh.Value.PortfolioFile!.MutualFundId}")))
                .Where(fh => fh.Value.Weight > 0) // Only consider funds that actually had the security
                .ToList();

            foreach (var removedFund in removedFunds)
            {
                fundChanges.Add(new SecurityFundChangeDto
                {
                    MutualFundId = removedFund.Value.PortfolioFile!.MutualFundId,
                    MutualFundName = removedFund.Value.PortfolioFile.MutualFund!.FundName,
                    PreviousWeight = removedFund.Value.Weight,
                    CurrentWeight = 0,
                    WeightChange = -removedFund.Value.Weight,
                    ChangeType = "Removed"
                });
            }

            // Group changes by type for this month
            if (fundChanges.Any())
            {
                var changeTypeSummaries = fundChanges
                    .GroupBy(fc => fc.ChangeType)
                    .Select(g => new MonthlyChangeTypeDto
                    {
                        ChangeType = g.Key,
                        MutualFundsCount = g.Count(),
                        TotalWeightChange = Math.Abs(g.Sum(fc => fc.WeightChange))
                    })
                    .ToList();

                monthlyChanges.Add(new SecurityMonthlyChangeDto
                {
                    MonthYear = currentMonth,
                    ChangeTypes = changeTypeSummaries,
                    TotalMutualFundsCount = fundChanges.Count,
                    TotalAbsoluteWeightChange = fundChanges.Sum(fc => Math.Abs(fc.WeightChange)),
                    AllFundChanges = fundChanges.OrderBy(fc => fc.MutualFundName).ToList()
                });
            }

            currentMonth = currentMonth.AddMonths(1);
        }

        return monthlyChanges;
    }

    private string DetermineChangeType(decimal fromWeight, decimal toWeight)
    {
        const decimal threshold = 0.001m; // 0.01% threshold

        if (fromWeight == 0 && toWeight > 0)
            return "Added";

        if (fromWeight > 0 && toWeight == 0)
            return "Removed";

        if (Math.Abs(toWeight - fromWeight) <= threshold)
            return "Unchanged";

        return toWeight > fromWeight ? "Increased" : "Decreased";
    }

    private List<SecurityMonthlyChangeDto> ApplySorting(
        List<SecurityMonthlyChangeDto> changes,
        string? sortBy,
        string? sortDirection)
    {
        if (string.IsNullOrEmpty(sortBy))
        {
            return changes.OrderByDescending(c => c.MonthYear).ToList();
        }

        var isAscending = sortDirection?.ToLower() == "asc";

        return sortBy.ToLower() switch
        {
            "date" or "monthyear" => isAscending
                ? changes.OrderBy(c => c.MonthYear).ToList()
                : changes.OrderByDescending(c => c.MonthYear).ToList(),

            "totalmutualfundscount" or "mutualfundscount" => isAscending
                ? changes.OrderBy(c => c.TotalMutualFundsCount).ToList()
                : changes.OrderByDescending(c => c.TotalMutualFundsCount).ToList(),

            "totalabsoluteweightchange" or "totalweightchange" => isAscending
                ? changes.OrderBy(c => c.TotalAbsoluteWeightChange).ToList()
                : changes.OrderByDescending(c => c.TotalAbsoluteWeightChange).ToList(),

            _ => changes.OrderByDescending(c => c.MonthYear).ToList()
        };
    }

    private SecurityAnalyticsSummaryDto CalculateSummary(List<SecurityMonthlyChangeDto> monthlyChanges)
    {
        var changeTypeCounts = new Dictionary<string, int>();
        var totalAbsoluteWeightChange = 0m;
        var totalMutualFunds = new HashSet<int>();

        foreach (var change in monthlyChanges)
        {
            // Count change types across all months
            foreach (var changeType in change.ChangeTypes)
            {
                if (changeTypeCounts.ContainsKey(changeType.ChangeType))
                    changeTypeCounts[changeType.ChangeType]++;
                else
                    changeTypeCounts[changeType.ChangeType] = 1;
            }

            // Sum absolute weight changes
            totalAbsoluteWeightChange += change.TotalAbsoluteWeightChange;

            // Collect unique mutual funds
            foreach (var fundChange in change.AllFundChanges)
            {
                totalMutualFunds.Add(fundChange.MutualFundId);
            }
        }

        return new SecurityAnalyticsSummaryDto
        {
            TotalMonthsAnalyzed = monthlyChanges.Count,
            MonthsWithChanges = monthlyChanges.Count,
            TotalMutualFundsInvolved = totalMutualFunds.Count,
            TotalAbsoluteWeightChange = totalAbsoluteWeightChange,
            ChangeTypeCounts = changeTypeCounts
        };
    }
}
