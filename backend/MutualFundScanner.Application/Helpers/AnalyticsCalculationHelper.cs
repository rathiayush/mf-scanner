using MutualFundScanner.Application.DTOs;
using MutualFundScanner.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace MutualFundScanner.Application.Helpers;

/// <summary>
/// Refactored helper class for calculating analytics changes with cleaner, more maintainable logic
/// </summary>
public class AnalyticsCalculationHelper
{
    private readonly ILogger<AnalyticsCalculationHelper> _logger;
    private const decimal WEIGHT_THRESHOLD = 0.01m; // 0.01% threshold for change detection

    public AnalyticsCalculationHelper(ILogger<AnalyticsCalculationHelper> logger)
    {
        _logger = logger;
    }

    #region Core Data Structures

    /// <summary>
    /// Represents a security holding for a specific fund at a specific time
    /// </summary>
    private class FundSecurityHolding
    {
        public int FundId { get; set; }
        public string FundName { get; set; } = "";
        public string ISIN { get; set; } = "";
        public string SecurityName { get; set; } = "";
        public string Sector { get; set; } = "";
        public string Industry { get; set; } = "";
        public decimal Weight { get; set; }
        public bool Exists { get; set; } // True if security exists in snapshot, regardless of weight
    }

    /// <summary>
    /// Represents the change in a security holding for a specific fund
    /// </summary>
    private class FundSecurityChange
    {
        public int FundId { get; set; }
        public string FundName { get; set; } = "";
        public string ISIN { get; set; } = "";
        public string SecurityName { get; set; } = "";
        public string Sector { get; set; } = "";
        public string Industry { get; set; } = "";
        public decimal FromWeight { get; set; }
        public decimal ToWeight { get; set; }
        public decimal WeightChange => ToWeight - FromWeight;
        public string ChangeType { get; set; } = "";
    }

    #endregion

    #region Unified Change Detection Logic

    /// <summary>
    /// Determines change type based on weight values with consistent logic
    /// </summary>
    private string DetermineChangeType(decimal fromWeight, decimal toWeight)
    {
        if (fromWeight == 0 && toWeight > 0) return "Added";
        if (fromWeight > 0 && toWeight == 0) return "Removed";
        
        var weightDiff = toWeight - fromWeight;
        if (Math.Abs(weightDiff) < WEIGHT_THRESHOLD) return "Unchanged";
        
        return weightDiff > 0 ? "Increased" : "Decreased";
    }

    /// <summary>
    /// Determines overall change type when multiple funds have different changes for same security
    /// </summary>
    private string DetermineOverallChangeType(List<string> individualChangeTypes, decimal totalFromWeight, decimal totalToWeight)
    {
        var uniqueTypes = individualChangeTypes.Distinct().ToList();
        
        // If multiple different change types exist, it's mixed
        if (uniqueTypes.Count > 1) return "Mixed";
        
        // If all funds have same change type, verify against total weights
        var singleType = uniqueTypes.FirstOrDefault() ?? "Unchanged";
        var weightBasedType = DetermineChangeType(totalFromWeight, totalToWeight);
        
        // If weight-based determination differs from individual types, it's mixed
        if (weightBasedType == "Unchanged" && singleType != "Unchanged") return "Mixed";
        
        return weightBasedType;
    }

    #endregion

    #region Data Preparation Methods

    /// <summary>
    /// Extracts fund holdings from snapshots with proper handling of missing securities
    /// </summary>
    private Dictionary<(int FundId, string ISIN), FundSecurityHolding> ExtractFundHoldings(
        List<PortfolioSnapshot> snapshots, 
        Dictionary<int, string> fundNames)
    {
        var holdings = new Dictionary<(int, string), FundSecurityHolding>();
        
        foreach (var snapshot in snapshots)
        {
            var fundName = fundNames.GetValueOrDefault(snapshot.MutualFundId, "Unknown Fund");
            
            foreach (var holding in snapshot.Holdings.Where(h => !string.IsNullOrEmpty(h.ISIN)))
            {
                var key = (snapshot.MutualFundId, holding.ISIN!);
                holdings[key] = new FundSecurityHolding
                {
                    FundId = snapshot.MutualFundId,
                    FundName = fundName,
                    ISIN = holding.ISIN!,
                    SecurityName = holding.SecurityName ?? "",
                    Sector = holding.Sector ?? "",
                    Industry = holding.Industry ?? "",
                    Weight = holding.Weight,
                    Exists = true
                };
            }
        }
        
        return holdings;
    }

    #endregion

    /// <summary>
    /// Main method: Calculates holding changes from snapshots with clean, maintainable logic
    /// </summary>
    public List<HoldingChangeDto> CalculateHoldingChangesFromSnapshots(
        List<PortfolioSnapshot> fromSnapshots,
        List<PortfolioSnapshot> toSnapshots,
        Dictionary<int, string> fundNames,
        DateTime fromDate,
        DateTime toDate)
    {
        // Extract holdings from snapshots
        var fromHoldings = ExtractFundHoldings(fromSnapshots, fundNames);
        var toHoldings = ExtractFundHoldings(toSnapshots, fundNames);
        
        // Get all unique ISINs and fund combinations
        var allKeys = fromHoldings.Keys.Union(toHoldings.Keys).ToList();
        
        // Calculate individual fund changes
        var fundChanges = new List<FundSecurityChange>();
        
        foreach (var (fundId, isin) in allKeys)
        {
            var fromHolding = fromHoldings.GetValueOrDefault((fundId, isin));
            var toHolding = toHoldings.GetValueOrDefault((fundId, isin));
            
            // Skip if no meaningful change occurred
            if (fromHolding == null && toHolding == null) continue;
            
            var fromWeight = fromHolding?.Weight ?? 0;
            var toWeight = toHolding?.Weight ?? 0;
            var changeType = DetermineChangeType(fromWeight, toWeight);
            
            if (changeType == "Unchanged") continue;
            
            // Get security details from whichever holding exists
            var securityInfo = toHolding ?? fromHolding!;
            
            fundChanges.Add(new FundSecurityChange
            {
                FundId = fundId,
                FundName = fundNames[fundId],
                ISIN = isin,
                SecurityName = securityInfo.SecurityName,
                Sector = securityInfo.Sector,
                Industry = securityInfo.Industry,
                FromWeight = fromWeight,
                ToWeight = toWeight,
                ChangeType = changeType
            });
        }
        
        // Convert to DTOs
        return fundChanges.Select(change => new HoldingChangeDto
        {
            ISIN = change.ISIN,
            SecurityName = change.SecurityName,
            Sector = change.Sector,
            Industry = change.Industry,
            PreviousWeight = change.FromWeight,
            CurrentWeight = change.ToWeight,
            WeightChange = change.WeightChange,
            ChangeType = change.ChangeType,
            MutualFundNames = new List<string> { change.FundName },
            FundChangeDetails = new List<FundChangeDetailDto>
            {
                new FundChangeDetailDto
                {
                    MutualFundId = change.FundId,
                    MutualFundName = change.FundName,
                    PreviousWeight = change.FromWeight,
                    CurrentWeight = change.ToWeight,
                    WeightChange = change.WeightChange,
                    ChangeType = change.ChangeType
                }
            },
            FromDate = fromDate,
            ToDate = toDate
        }).ToList();
    }

    /// <summary>
    /// Groups holding changes by ISIN with enhanced mixed scenario detection
    /// </summary>
    public List<HoldingChangeDto> GroupChangesByISIN(List<HoldingChangeDto> changes)
    {
        return changes
            .GroupBy(c => c.ISIN)
            .Select(g => {
                var fundChanges = g.ToList();
                var totalPreviousWeight = fundChanges.Sum(c => c.PreviousWeight ?? 0);
                var totalCurrentWeight = fundChanges.Sum(c => c.CurrentWeight);
                var totalWeightChange = totalCurrentWeight - totalPreviousWeight;
                
                // Get individual change types
                var individualChangeTypes = fundChanges.Select(c => c.ChangeType).ToList();
                var overallChangeType = DetermineOverallChangeType(individualChangeTypes, totalPreviousWeight, totalCurrentWeight);
                
                return new HoldingChangeDto
                {
                    ISIN = g.Key,
                    SecurityName = g.First().SecurityName,
                    Sector = g.First().Sector,
                    Industry = g.First().Industry,
                    PreviousWeight = totalPreviousWeight,
                    CurrentWeight = totalCurrentWeight,
                    WeightChange = totalWeightChange,
                    ChangeType = overallChangeType,
                    MutualFundNames = g.SelectMany(c => c.MutualFundNames).Distinct().OrderBy(name => name).ToList(),
                    FundChangeDetails = fundChanges.SelectMany(fc => fc.FundChangeDetails).ToList(),
                    FromDate = g.First().FromDate,
                    ToDate = g.First().ToDate
                };
            })
            .Where(c => c.ChangeType != "Unchanged")
            .OrderByDescending(c => Math.Abs(c.WeightChange))
            .ToList();
    }

    #region Legacy Methods (Maintained for Compatibility)

    /// <summary>
    /// Legacy method for calculating holding changes (maintained for compatibility)
    /// </summary>
    public List<HoldingChangeDto> CalculateHoldingChanges(
        Dictionary<string, PortfolioHolding> fromHoldings,
        Dictionary<string, PortfolioHolding> toHoldings,
        List<string> fundNames,
        DateTime fromDate,
        DateTime toDate)
    {
        var changes = new List<HoldingChangeDto>();
        var allIsins = fromHoldings.Keys.Union(toHoldings.Keys);

        foreach (var isin in allIsins)
        {
            fromHoldings.TryGetValue(isin, out var fromHolding);
            toHoldings.TryGetValue(isin, out var toHolding);

            var fromWeight = fromHolding?.Weight ?? 0;
            var toWeight = toHolding?.Weight ?? 0;
            var changeType = DetermineChangeType(fromWeight, toWeight);

            if (changeType == "Unchanged") continue;

            var securityInfo = toHolding ?? fromHolding!;
            changes.Add(new HoldingChangeDto
            {
                ISIN = isin,
                SecurityName = securityInfo.SecurityName ?? "",
                Sector = securityInfo.Sector ?? "",
                Industry = securityInfo.Industry ?? "",
                PreviousWeight = fromWeight,
                CurrentWeight = toWeight,
                WeightChange = toWeight - fromWeight,
                ChangeType = changeType,
                MutualFundNames = fundNames,
                FromDate = fromDate,
                ToDate = toDate
            });
        }

        return changes;
    }

    /// <summary>
    /// Applies filtering and pagination to holding changes
    /// </summary>
    public PagedResult<HoldingChangeDto> ApplyFilteringAndPagination(
        List<HoldingChangeDto> changes,
        string? changeTypeFilter,
        int page,
        int pageSize)
    {
        var orderedChanges = changes.OrderByDescending(c => Math.Abs(c.WeightChange)).ToList();

        var filteredChanges = orderedChanges;
        if (!string.IsNullOrEmpty(changeTypeFilter))
        {
            _logger.LogInformation("Applying change type filter: {ChangeTypeFilter}", changeTypeFilter);
            filteredChanges = orderedChanges.Where(c => c.ChangeType == changeTypeFilter).ToList();
            _logger.LogInformation("Filtered changes count: {FilteredCount} from original count: {OriginalCount}", 
                filteredChanges.Count, orderedChanges.Count);
        }

        var totalCount = filteredChanges.Count;
        var pagedChanges = filteredChanges.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        return new PagedResult<HoldingChangeDto>
        {
            Data = pagedChanges,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    /// <summary>
    /// Creates a summary of analytics changes
    /// </summary>
    public AnalyticsSummaryDto CreateAnalyticsSummary(List<HoldingChangeDto> changes)
    {
        return new AnalyticsSummaryDto
        {
            TotalHoldingsAnalyzed = changes.Count,
            AddedHoldings = changes.Count(c => c.ChangeType == "Added"),
            RemovedHoldings = changes.Count(c => c.ChangeType == "Removed"),
            IncreasedHoldings = changes.Count(c => c.ChangeType == "Increased"),
            DecreasedHoldings = changes.Count(c => c.ChangeType == "Decreased"),
            UnchangedHoldings = changes.Count(c => c.ChangeType == "Unchanged"),
            TotalWeightChange = changes.Sum(c => Math.Abs(c.WeightChange))
        };
    }

    /// <summary>
    /// Creates top shares data from individual snapshots
    /// </summary>
    public List<TopShareDto> CreateTopSharesFromSnapshots(
        List<PortfolioSnapshot> snapshots,
        Dictionary<int, string> fundNames,
        int topCount = 15)
    {
        var allHoldings = new Dictionary<string, TopShareDto>();

        foreach (var snapshot in snapshots)
        {
            foreach (var holding in snapshot.Holdings.Where(h => !string.IsNullOrEmpty(h.ISIN)))
            {
                var isin = holding.ISIN!;
                if (!allHoldings.ContainsKey(isin))
                {
                    allHoldings[isin] = new TopShareDto
                    {
                        ISIN = isin,
                        SecurityName = holding.SecurityName ?? "",
                        Sector = holding.Sector ?? "",
                        Industry = holding.Industry ?? "",
                        TotalWeight = 0,
                        HoldingFundsCount = 0,
                        FundHoldings = new List<FundHoldingDto>()
                    };
                }

                var fundName = fundNames.GetValueOrDefault(snapshot.MutualFundId, "Unknown Fund");
                allHoldings[isin].FundHoldings.Add(new FundHoldingDto
                {
                    MutualFundId = snapshot.MutualFundId,
                    MutualFundName = fundName,
                    Weight = holding.Weight
                });

                allHoldings[isin].TotalWeight += holding.Weight;
                allHoldings[isin].HoldingFundsCount = allHoldings[isin].FundHoldings.Count;
            }
        }

        return allHoldings.Values.OrderByDescending(s => s.TotalWeight).Take(topCount).ToList();
    }

    /// <summary>
    /// Creates top shares data from holdings (legacy method)
    /// </summary>
    public List<TopShareDto> CreateTopSharesFromHoldings(
        Dictionary<string, PortfolioHolding> aggregatedHoldings,
        Dictionary<int, string> fundNames)
    {
        return aggregatedHoldings.Select(kvp => new TopShareDto
        {
            ISIN = kvp.Key,
            SecurityName = kvp.Value.SecurityName ?? "",
            Sector = kvp.Value.Sector ?? "",
            Industry = kvp.Value.Industry ?? "",
            TotalWeight = kvp.Value.Weight,
            HoldingFundsCount = fundNames.Count,
            FundHoldings = fundNames.Select(fn => new FundHoldingDto
            {
                MutualFundId = fn.Key,
                MutualFundName = fn.Value,
                Weight = kvp.Value.Weight / fundNames.Count
            }).ToList()
        }).OrderByDescending(s => s.TotalWeight).Take(15).ToList();
    }

    #endregion
}
