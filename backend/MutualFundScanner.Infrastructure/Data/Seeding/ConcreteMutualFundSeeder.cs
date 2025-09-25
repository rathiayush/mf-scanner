using Microsoft.EntityFrameworkCore;
using MutualFundScanner.Domain.Entities;

namespace MutualFundScanner.Infrastructure.Data.Seeding;

public static class ConcreteMutualFundSeeder
{
    public static void Seed(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ConcreteMutualFund>().HasData(
            new ConcreteMutualFund
            {
                Id = 1,
                FundName = "Axis Bluechip Fund",
                AMCName = "Axis Mutual Fund",
                Category = "Equity",
                SubCategory = "Large Cap",
                MatchPriority = 1,
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                LastUpdated = DateTime.UtcNow,
                Notes = "Large cap equity fund"
            },
            new ConcreteMutualFund
            {
                Id = 2,
                FundName = "HDFC Top 100 Fund",
                AMCName = "HDFC Mutual Fund",
                Category = "Equity",
                SubCategory = "Large Cap",
                MatchPriority = 1,
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                LastUpdated = DateTime.UtcNow,
                Notes = "Large cap equity fund"
            },
            new ConcreteMutualFund
            {
                Id = 3,
                FundName = "ICICI Prudential Bluechip Fund",
                AMCName = "ICICI Prudential Mutual Fund",
                Category = "Equity",
                SubCategory = "Large Cap",
                MatchPriority = 1,
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                LastUpdated = DateTime.UtcNow,
                Notes = "Large cap equity fund"
            },
            new ConcreteMutualFund
            {
                Id = 4,
                FundName = "SBI Bluechip Fund",
                AMCName = "SBI Mutual Fund",
                Category = "Equity",
                SubCategory = "Large Cap",
                MatchPriority = 1,
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                LastUpdated = DateTime.UtcNow,
                Notes = "Large cap equity fund"
            },
            new ConcreteMutualFund
            {
                Id = 5,
                FundName = "Mirae Asset Large Cap Fund",
                AMCName = "Mirae Asset Mutual Fund",
                Category = "Equity",
                SubCategory = "Large Cap",
                MatchPriority = 1,
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                LastUpdated = DateTime.UtcNow,
                Notes = "Large cap equity fund"
            },
            new ConcreteMutualFund
            {
                Id = 6,
                FundName = "Axis Midcap Fund",
                AMCName = "Axis Mutual Fund",
                Category = "Equity",
                SubCategory = "Mid Cap",
                MatchPriority = 2,
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                LastUpdated = DateTime.UtcNow,
                Notes = "Mid cap equity fund"
            },
            new ConcreteMutualFund
            {
                Id = 7,
                FundName = "HDFC Mid-Cap Opportunities Fund",
                AMCName = "HDFC Mutual Fund",
                Category = "Equity",
                SubCategory = "Mid Cap",
                MatchPriority = 2,
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                LastUpdated = DateTime.UtcNow,
                Notes = "Mid cap equity fund"
            },
            new ConcreteMutualFund
            {
                Id = 8,
                FundName = "ICICI Prudential Midcap Fund",
                AMCName = "ICICI Prudential Mutual Fund",
                Category = "Equity",
                SubCategory = "Mid Cap",
                MatchPriority = 2,
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                LastUpdated = DateTime.UtcNow,
                Notes = "Mid cap equity fund"
            },
            new ConcreteMutualFund
            {
                Id = 9,
                FundName = "SBI Magnum Midcap Fund",
                AMCName = "SBI Mutual Fund",
                Category = "Equity",
                SubCategory = "Mid Cap",
                MatchPriority = 2,
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                LastUpdated = DateTime.UtcNow,
                Notes = "Mid cap equity fund"
            },
            new ConcreteMutualFund
            {
                Id = 10,
                FundName = "Axis Small Cap Fund",
                AMCName = "Axis Mutual Fund",
                Category = "Equity",
                SubCategory = "Small Cap",
                MatchPriority = 3,
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                LastUpdated = DateTime.UtcNow,
                Notes = "Small cap equity fund"
            },
            new ConcreteMutualFund
            {
                Id = 11,
                FundName = "HDFC Small Cap Fund",
                AMCName = "HDFC Mutual Fund",
                Category = "Equity",
                SubCategory = "Small Cap",
                MatchPriority = 3,
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                LastUpdated = DateTime.UtcNow,
                Notes = "Small cap equity fund"
            },
            new ConcreteMutualFund
            {
                Id = 12,
                FundName = "ICICI Prudential Smallcap Fund",
                AMCName = "ICICI Prudential Mutual Fund",
                Category = "Equity",
                SubCategory = "Small Cap",
                MatchPriority = 3,
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                LastUpdated = DateTime.UtcNow,
                Notes = "Small cap equity fund"
            },
            new ConcreteMutualFund
            {
                Id = 13,
                FundName = "SBI Small Cap Fund",
                AMCName = "SBI Mutual Fund",
                Category = "Equity",
                SubCategory = "Small Cap",
                MatchPriority = 3,
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                LastUpdated = DateTime.UtcNow,
                Notes = "Small cap equity fund"
            },
            new ConcreteMutualFund
            {
                Id = 14,
                FundName = "Axis Liquid Fund",
                AMCName = "Axis Mutual Fund",
                Category = "Debt",
                SubCategory = "Liquid",
                MatchPriority = 4,
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                LastUpdated = DateTime.UtcNow,
                Notes = "Liquid debt fund"
            },
            new ConcreteMutualFund
            {
                Id = 15,
                FundName = "HDFC Liquid Fund",
                AMCName = "HDFC Mutual Fund",
                Category = "Debt",
                SubCategory = "Liquid",
                MatchPriority = 4,
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                LastUpdated = DateTime.UtcNow,
                Notes = "Liquid debt fund"
            },
            new ConcreteMutualFund
            {
                Id = 16,
                FundName = "ICICI Prudential Liquid Fund",
                AMCName = "ICICI Prudential Mutual Fund",
                Category = "Debt",
                SubCategory = "Liquid",
                MatchPriority = 4,
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                LastUpdated = DateTime.UtcNow,
                Notes = "Liquid debt fund"
            },
            new ConcreteMutualFund
            {
                Id = 17,
                FundName = "SBI Liquid Fund",
                AMCName = "SBI Mutual Fund",
                Category = "Debt",
                SubCategory = "Liquid",
                MatchPriority = 4,
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                LastUpdated = DateTime.UtcNow,
                Notes = "Liquid debt fund"
            },
            new ConcreteMutualFund
            {
                Id = 18,
                FundName = "Axis Arbitrage Fund",
                AMCName = "Axis Mutual Fund",
                Category = "Hybrid",
                SubCategory = "Arbitrage",
                MatchPriority = 5,
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                LastUpdated = DateTime.UtcNow,
                Notes = "Arbitrage hybrid fund"
            },
            new ConcreteMutualFund
            {
                Id = 19,
                FundName = "HDFC Arbitrage Fund",
                AMCName = "HDFC Mutual Fund",
                Category = "Hybrid",
                SubCategory = "Arbitrage",
                MatchPriority = 5,
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                LastUpdated = DateTime.UtcNow,
                Notes = "Arbitrage hybrid fund"
            },
            new ConcreteMutualFund
            {
                Id = 20,
                FundName = "ICICI Prudential Equity Arbitrage Fund",
                AMCName = "ICICI Prudential Mutual Fund",
                Category = "Hybrid",
                SubCategory = "Arbitrage",
                MatchPriority = 5,
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                LastUpdated = DateTime.UtcNow,
                Notes = "Arbitrage hybrid fund"
            }
        );
    }
}
