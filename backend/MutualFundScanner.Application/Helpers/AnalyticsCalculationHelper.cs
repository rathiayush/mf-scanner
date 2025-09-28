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
    public class FundSecurityChange
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
        // If from weight is 0 and to weight is positive, it's added
        if (fromWeight == 0 && toWeight > 0) return "Added";
        
        // If from weight is positive and to weight is 0, it's removed
        if (fromWeight > 0 && toWeight == 0) return "Removed";
        
        // If both weights are 0, it's unchanged
        if (fromWeight == 0 && toWeight == 0) return "Unchanged";
        
        // Both weights are positive, check for significant change
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
                
                // Check if this is a "to" snapshot with NULL weights (indicating removal)
                // We need to distinguish between 0 weight and NULL/missing data
                var weight = holding.Weight;
                
                holdings[key] = new FundSecurityHolding
                {
                    FundId = snapshot.MutualFundId,
                    FundName = fundName,
                    ISIN = holding.ISIN!,
                    SecurityName = holding.SecurityName ?? "",
                    Sector = holding.Sector ?? "",
                    Industry = holding.Industry ?? "",
                    Weight = weight,
                    Exists = weight > 0m // Only exists if weight is positive
                };
            }
        }
        
        return holdings;
    }

    #endregion

    /// <summary>
    /// Main method: Calculates holding changes from snapshots with clean, maintainable logic
    /// </summary>
    public List<FundSecurityChange> CalculateHoldingChangesFromSnapshots(
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
            
            // Get weights - handle the case where holdings might exist but have 0 weight
            var fromWeight = fromHolding?.Weight ?? 0;
            var toWeight = toHolding?.Weight ?? 0;
            
            // Special handling for the case where both holdings exist but toWeight is 0
            // This could indicate a removal OR a data issue
            string changeType;
            if (fromHolding != null && toHolding != null && fromWeight > 0 && toWeight == 0)
            {
                // Check if this is truly a removal or a data issue
                // For now, we'll treat it as unchanged if both holdings exist
                // This prevents false "Removed" detections
                _logger.LogWarning("Potential data issue: Fund {FundId} ({FundName}) has holding in both snapshots but toWeight is 0. ISIN: {ISIN}", 
                    fundId, fundNames[fundId], isin);
                
                // Treat as unchanged rather than removed to avoid false positives
                changeType = "Unchanged";
            }
            else
            {
                // Log for debugging INOX Wind Limited
                if (isin == "INE066P01011")
                {
                    _logger.LogInformation("Fund {FundId} ({FundName}): FromWeight={FromWeight}, ToWeight={ToWeight}", 
                        fundId, fundNames[fundId], fromWeight, toWeight);
                }
                
                changeType = DetermineChangeType(fromWeight, toWeight);
            }
            
            // Include all changes, even unchanged ones, for proper aggregation
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
        
        return fundChanges;
    }

    /// <summary>
    /// Converts fund security changes to holding change DTOs
    /// </summary>
    public List<HoldingChangeDto> ConvertFundChangesToDto(List<FundSecurityChange> fundChanges, DateTime fromDate, DateTime toDate)
    {
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
            ConstituentChangeTypes = new List<string> { change.ChangeType }, // Single fund, single change type
            FromDate = fromDate,
            ToDate = toDate
        }).ToList();
    }

    /// <summary>
    /// Groups holding changes by ISIN with enhanced mixed scenario detection
    /// </summary>
    public List<HoldingChangeDto> GroupChangesByISIN(List<FundSecurityChange> fundChanges, DateTime fromDate, DateTime toDate)
    {
        return fundChanges
            .GroupBy(c => c.ISIN)
            .Select(g => {
                var changesForIsin = g.ToList();
                var totalPreviousWeight = changesForIsin.Sum(c => c.FromWeight);
                var totalCurrentWeight = changesForIsin.Sum(c => c.ToWeight);
                var totalWeightChange = totalCurrentWeight - totalPreviousWeight;
                
                // Get individual change types
                var individualChangeTypes = changesForIsin.Select(c => c.ChangeType).ToList();
                var overallChangeType = DetermineOverallChangeType(individualChangeTypes, totalPreviousWeight, totalCurrentWeight);
                
                // Create fund change details
                var fundChangeDetails = changesForIsin.Select(fc => new FundChangeDetailDto
                {
                    MutualFundId = fc.FundId,
                    MutualFundName = fc.FundName,
                    PreviousWeight = fc.FromWeight,
                    CurrentWeight = fc.ToWeight,
                    WeightChange = fc.WeightChange,
                    ChangeType = fc.ChangeType
                }).ToList();
                
                // Get constituent change types for Mixed scenarios
                var constituentChangeTypes = new List<string>();
                if (overallChangeType == "Mixed" && fundChangeDetails.Any())
                {
                    constituentChangeTypes = fundChangeDetails
                        .Select(fd => fd.ChangeType)
                        .Distinct()
                        .Where(ct => ct != "Unchanged")
                        .OrderBy(ct => ct)
                        .ToList();
                }
                
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
                    MutualFundNames = changesForIsin.Select(c => c.FundName).Distinct().OrderBy(name => name).ToList(),
                    FundChangeDetails = fundChangeDetails.OrderBy(fd => fd.MutualFundName).ToList(),
                    ConstituentChangeTypes = constituentChangeTypes,
                    FromDate = fromDate,
                    ToDate = toDate
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
    /// Applies filtering, sorting and pagination to holding changes
    /// </summary>
    public PagedResult<HoldingChangeDto> ApplyFilteringAndPagination(
        List<HoldingChangeDto> changes,
        string? changeTypeFilter,
        int page,
        int pageSize,
        string? sortBy = null,
        string? sortDirection = "desc")
    {
        // Apply enhanced filtering that includes Mixed changes with constituent types
        var filteredChanges = changes;
        if (!string.IsNullOrEmpty(changeTypeFilter))
        {
            _logger.LogInformation("Applying change type filter: {ChangeTypeFilter}", changeTypeFilter);
            
            filteredChanges = changes.Where(c => 
            {
                // Direct match
                if (c.ChangeType == changeTypeFilter) return true;
                
                // For Mixed changes, check if any fund has the requested change type
                if (c.ChangeType == "Mixed" && c.FundChangeDetails != null)
                {
                    return c.FundChangeDetails.Any(fd => fd.ChangeType == changeTypeFilter);
                }
                
                return false;
            }).ToList();
            
            _logger.LogInformation("Enhanced filtered changes count: {FilteredCount} from original count: {OriginalCount} (includes Mixed with {ChangeTypeFilter})", 
                filteredChanges.Count, changes.Count, changeTypeFilter);
        }

        // Apply sorting
        var sortedChanges = ApplySorting(filteredChanges, sortBy, sortDirection);

        var totalCount = sortedChanges.Count;
        var pagedChanges = sortedChanges.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        return new PagedResult<HoldingChangeDto>
        {
            Data = pagedChanges,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    /// <summary>
    /// Applies sorting to holding changes based on specified column and direction
    /// </summary>
    private List<HoldingChangeDto> ApplySorting(List<HoldingChangeDto> changes, string? sortBy, string? sortDirection)
    {
        if (string.IsNullOrEmpty(sortBy))
        {
            // Default sorting by absolute weight change descending
            return changes.OrderByDescending(c => Math.Abs(c.WeightChange)).ToList();
        }

        var isAscending = sortDirection?.ToLower() == "asc";
        
        return sortBy.ToLower() switch
        {
            "securityname" => isAscending 
                ? changes.OrderBy(c => c.SecurityName).ToList()
                : changes.OrderByDescending(c => c.SecurityName).ToList(),
            
            "sector" => isAscending 
                ? changes.OrderBy(c => c.Sector).ToList()
                : changes.OrderByDescending(c => c.Sector).ToList(),
            
            "industry" => isAscending 
                ? changes.OrderBy(c => c.Industry).ToList()
                : changes.OrderByDescending(c => c.Industry).ToList(),
            
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
            
            "isin" => isAscending 
                ? changes.OrderBy(c => c.ISIN).ToList()
                : changes.OrderByDescending(c => c.ISIN).ToList(),
            
            _ => changes.OrderByDescending(c => Math.Abs(c.WeightChange)).ToList() // Default fallback
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
        int? topCount = null)
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

        var orderedHoldings = allHoldings.Values.OrderByDescending(s => s.TotalWeight);
        return topCount.HasValue ? orderedHoldings.Take(topCount.Value).ToList() : orderedHoldings.ToList();
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
