using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MutualFundScanner.Domain.Entities;

namespace MutualFundScanner.Infrastructure.Data.Configurations
{
    public class ConcreteMutualFundSeedConfiguration : IEntityTypeConfiguration<ConcreteMutualFund>
    {
        public void Configure(EntityTypeBuilder<ConcreteMutualFund> builder)
        {
            // Seed data for ConcreteMutualFund
            builder.HasData(GetConcreteMutualFundSeedData());
        }

        private List<ConcreteMutualFund> GetConcreteMutualFundSeedData()
        {
            var databaseEntities = new List<ConcreteMutualFund>();
            int id = 1;
            
            // Define seed data structure grouped by AMC
            var amcFunds = new[]
            {
                new
                {
                    AMCName = "Axis Mutual Fund",
                    Funds = new[]
                    {
                        new { FundName = "Axis Bluechip Fund", Category = "Equity", SubCategory = "Large Cap", Notes = "Large cap fund focusing on established companies", MatchPriority = 90 },
                        new { FundName = "Axis Midcap Fund", Category = "Equity", SubCategory = "Mid Cap", Notes = "Mid cap fund focusing on growth opportunities", MatchPriority = 85 },
                        new { FundName = "Axis Small Cap Fund", Category = "Equity", SubCategory = "Small Cap", Notes = "Small cap fund focusing on high growth potential", MatchPriority = 80 },
                        new { FundName = "Axis Multicap Fund", Category = "Equity", SubCategory = "Multi Cap", Notes = "Multi cap fund with flexible allocation across market caps", MatchPriority = 85 },
                        new { FundName = "Axis Long Term Equity Fund", Category = "Equity", SubCategory = "ELSS", Notes = "ELSS fund with tax benefits under Section 80C", MatchPriority = 85 },
                        new { FundName = "Axis Liquid Fund", Category = "Debt", SubCategory = "Liquid", Notes = "Liquid fund for short-term investments", MatchPriority = 80 },
                        new { FundName = "Axis Nifty 50 Index Fund", Category = "Equity", SubCategory = "Index", Notes = "Index fund tracking Nifty 50", MatchPriority = 80 },
                        new { FundName = "Axis Technology Fund", Category = "Equity", SubCategory = "Sector", Notes = "Sector fund focusing on technology companies", MatchPriority = 75 },
                        new { FundName = "Axis Banking & Financial Services Fund", Category = "Equity", SubCategory = "Sector", Notes = "Sector fund focusing on banking and financial services", MatchPriority = 75 },
                        new { FundName = "Axis International Equity Fund", Category = "Equity", SubCategory = "International", Notes = "International equity fund with global exposure", MatchPriority = 70 }
                    }
                },
                new
                {
                    AMCName = "ICICI Prudential Mutual Fund",
                    Funds = new[]
                    {
                        new { FundName = "ICICI Prudential Bluechip Fund", Category = "Equity", SubCategory = "Large Cap", Notes = "Large cap equity fund with diversified portfolio", MatchPriority = 90 },
                        new { FundName = "ICICI Prudential Midcap Fund", Category = "Equity", SubCategory = "Mid Cap", Notes = "Mid cap fund with diversified portfolio", MatchPriority = 85 },
                        new { FundName = "ICICI Prudential Smallcap Fund", Category = "Equity", SubCategory = "Small Cap", Notes = "Small cap fund with diversified portfolio", MatchPriority = 80 },
                        new { FundName = "ICICI Prudential Multicap Fund", Category = "Equity", SubCategory = "Multi Cap", Notes = "Multi cap fund with diversified portfolio", MatchPriority = 85 },
                        new { FundName = "ICICI Prudential Long Term Equity Fund", Category = "Equity", SubCategory = "ELSS", Notes = "ELSS fund with diversified equity portfolio", MatchPriority = 85 },
                        new { FundName = "ICICI Prudential Liquid Fund", Category = "Debt", SubCategory = "Liquid", Notes = "Popular liquid fund for emergency funds", MatchPriority = 85 },
                        new { FundName = "ICICI Prudential Nifty Index Fund", Category = "Equity", SubCategory = "Index", Notes = "Index fund tracking Nifty 50", MatchPriority = 80 },
                        new { FundName = "ICICI Prudential Technology Fund", Category = "Equity", SubCategory = "Sector", Notes = "Sector fund focusing on technology and IT companies", MatchPriority = 75 },
                        new { FundName = "ICICI Prudential Banking & Financial Services Fund", Category = "Equity", SubCategory = "Sector", Notes = "Sector fund focusing on banking and financial sector", MatchPriority = 75 },
                        new { FundName = "ICICI Prudential US Bluechip Equity Fund", Category = "Equity", SubCategory = "International", Notes = "International fund focusing on US bluechip companies", MatchPriority = 70 }
                    }
                },
                new
                {
                    AMCName = "SBI Mutual Fund",
                    Funds = new[]
                    {
                        new { FundName = "SBI Bluechip Fund", Category = "Equity", SubCategory = "Large Cap", Notes = "Large cap fund with focus on quality companies", MatchPriority = 90 },
                        new { FundName = "SBI Magnum Midcap Fund", Category = "Equity", SubCategory = "Mid Cap", Notes = "Mid cap fund with focus on quality mid-sized companies", MatchPriority = 85 },
                        new { FundName = "SBI Small Cap Fund", Category = "Equity", SubCategory = "Small Cap", Notes = "Small cap fund with focus on quality small companies", MatchPriority = 80 },
                        new { FundName = "SBI Magnum Multicap Fund", Category = "Equity", SubCategory = "Multi Cap", Notes = "Multi cap fund with focus on quality companies", MatchPriority = 85 },
                        new { FundName = "SBI Magnum Tax Gain Fund", Category = "Equity", SubCategory = "ELSS", Notes = "ELSS fund with 3-year lock-in period", MatchPriority = 85 },
                        new { FundName = "SBI Liquid Fund", Category = "Debt", SubCategory = "Liquid", Notes = "Liquid fund with high safety and liquidity", MatchPriority = 85 },
                        new { FundName = "SBI Nifty Index Fund", Category = "Equity", SubCategory = "Index", Notes = "Index fund tracking Nifty 50", MatchPriority = 80 },
                        new { FundName = "SBI Technology Opportunities Fund", Category = "Equity", SubCategory = "Sector", Notes = "Sector fund focusing on technology sector", MatchPriority = 75 },
                        new { FundName = "SBI Banking & Financial Services Fund", Category = "Equity", SubCategory = "Sector", Notes = "Sector fund focusing on banking and financial services", MatchPriority = 75 },
                        new { FundName = "SBI International Equity Fund", Category = "Equity", SubCategory = "International", Notes = "International equity fund with global diversification", MatchPriority = 70 }
                    }
                },
                new
                {
                    AMCName = "HDFC Mutual Fund",
                    Funds = new[]
                    {
                        new { FundName = "HDFC Top 100 Fund", Category = "Equity", SubCategory = "Large Cap", Notes = "Large cap fund investing in top 100 companies", MatchPriority = 90 },
                        new { FundName = "HDFC Mid-Cap Opportunities Fund", Category = "Equity", SubCategory = "Mid Cap", Notes = "Mid cap fund focusing on growth opportunities", MatchPriority = 85 },
                        new { FundName = "HDFC Small Cap Fund", Category = "Equity", SubCategory = "Small Cap", Notes = "Small cap fund focusing on growth opportunities", MatchPriority = 80 },
                        new { FundName = "HDFC Equity Fund", Category = "Equity", SubCategory = "Multi Cap", Notes = "Multi cap fund with long-term growth focus", MatchPriority = 85 },
                        new { FundName = "HDFC Tax Saver Fund", Category = "Equity", SubCategory = "ELSS", Notes = "ELSS fund with tax-saving benefits", MatchPriority = 85 },
                        new { FundName = "HDFC Liquid Fund", Category = "Debt", SubCategory = "Liquid", Notes = "Liquid fund for short-term cash management", MatchPriority = 85 },
                        new { FundName = "HDFC Index Fund Nifty 50", Category = "Equity", SubCategory = "Index", Notes = "Index fund tracking Nifty 50", MatchPriority = 80 },
                        new { FundName = "HDFC IT Fund", Category = "Equity", SubCategory = "Sector", Notes = "Sector fund focusing on IT and technology", MatchPriority = 75 },
                        new { FundName = "HDFC Banking & Financial Services Fund", Category = "Equity", SubCategory = "Sector", Notes = "Sector fund focusing on banking and financial sector", MatchPriority = 75 },
                        new { FundName = "HDFC International Opportunities Fund", Category = "Equity", SubCategory = "International", Notes = "International fund focusing on global opportunities", MatchPriority = 70 }
                    }
                },
                new
                {
                    AMCName = "Mirae Asset Mutual Fund",
                    Funds = new[]
                    {
                        new { FundName = "Mirae Asset Large Cap Fund", Category = "Equity", SubCategory = "Large Cap", Notes = "Large cap fund with international exposure", MatchPriority = 90 },
                        new { FundName = "Mirae Asset Emerging Bluechip Fund", Category = "Equity", SubCategory = "Mid Cap", Notes = "Mid cap fund with focus on emerging companies", MatchPriority = 85 },
                        new { FundName = "Mirae Asset Multicap Fund", Category = "Equity", SubCategory = "Multi Cap", Notes = "Multi cap fund with flexible allocation strategy", MatchPriority = 85 },
                        new { FundName = "Mirae Asset Tax Saver Fund", Category = "Equity", SubCategory = "ELSS", Notes = "ELSS fund with focus on quality companies", MatchPriority = 85 },
                        new { FundName = "Mirae Asset Large Cap Fund", Category = "Equity", SubCategory = "Index", Notes = "Index fund tracking Nifty 50", MatchPriority = 80 },
                        new { FundName = "Mirae Asset Large Cap Fund", Category = "Equity", SubCategory = "International", Notes = "International equity fund with global exposure", MatchPriority = 70 }
                    }
                },
                new
                {
                    AMCName = "Nippon India Mutual Fund",
                    Funds = new[]
                    {
                        new { FundName = "Nippon India Large Cap Fund", Category = "Equity", SubCategory = "Large Cap", Notes = "Large cap fund with focus on quality companies", MatchPriority = 90 },
                        new { FundName = "Nippon India Growth Fund", Category = "Equity", SubCategory = "Mid Cap", Notes = "Mid cap fund with growth orientation", MatchPriority = 85 },
                        new { FundName = "Nippon India Small Cap Fund", Category = "Equity", SubCategory = "Small Cap", Notes = "Small cap fund with focus on emerging companies", MatchPriority = 80 },
                        new { FundName = "Nippon India Multi Cap Fund", Category = "Equity", SubCategory = "Multi Cap", Notes = "Multi cap fund with diversified portfolio", MatchPriority = 85 },
                        new { FundName = "Nippon India Tax Saver (ELSS) Fund", Category = "Equity", SubCategory = "ELSS", Notes = "ELSS fund with tax benefits", MatchPriority = 85 },
                        new { FundName = "Nippon India Liquid Fund", Category = "Debt", SubCategory = "Liquid", Notes = "Liquid fund with high credit quality", MatchPriority = 85 },
                        new { FundName = "Nippon India Index Fund - Nifty 50", Category = "Equity", SubCategory = "Index", Notes = "Index fund tracking Nifty 50", MatchPriority = 80 }
                    }
                },
                new
                {
                    AMCName = "UTI Mutual Fund",
                    Funds = new[]
                    {
                        new { FundName = "UTI Nifty Index Fund", Category = "Equity", SubCategory = "Index", Notes = "Index fund tracking Nifty 50", MatchPriority = 80 },
                        new { FundName = "UTI Mastershare Fund", Category = "Equity", SubCategory = "Large Cap", Notes = "Large cap fund with long track record", MatchPriority = 90 },
                        new { FundName = "UTI Mid Cap Fund", Category = "Equity", SubCategory = "Mid Cap", Notes = "Mid cap fund with focus on growth companies", MatchPriority = 85 },
                        new { FundName = "UTI Small Cap Fund", Category = "Equity", SubCategory = "Small Cap", Notes = "Small cap fund with high growth potential", MatchPriority = 80 },
                        new { FundName = "UTI Equity Fund", Category = "Equity", SubCategory = "Multi Cap", Notes = "Multi cap fund with diversified portfolio", MatchPriority = 85 },
                        new { FundName = "UTI Long Term Equity Fund", Category = "Equity", SubCategory = "ELSS", Notes = "ELSS fund with tax benefits", MatchPriority = 85 }
                    }
                },
                new
                {
                    AMCName = "TATA Mutual Fund",
                    Funds = new[]
                    {
                        new { FundName = "TATA Equity PE Fund", Category = "Equity", SubCategory = "Large Cap", Notes = "Large cap fund with PE investment strategy", MatchPriority = 90 },
                        new { FundName = "TATA Mid Cap Growth Fund", Category = "Equity", SubCategory = "Mid Cap", Notes = "Mid cap fund with growth focus", MatchPriority = 85 },
                        new { FundName = "TATA Small Cap Fund", Category = "Equity", SubCategory = "Small Cap", Notes = "Small cap fund with growth potential", MatchPriority = 80 },
                        new { FundName = "TATA Multicap Fund", Category = "Equity", SubCategory = "Multi Cap", Notes = "Multi cap fund with flexible allocation", MatchPriority = 85 },
                        new { FundName = "TATA Tax Saving Fund", Category = "Equity", SubCategory = "ELSS", Notes = "ELSS fund with tax benefits", MatchPriority = 85 },
                        new { FundName = "TATA Digital India Fund", Category = "Equity", SubCategory = "Sector", Notes = "Sector fund focusing on digital and technology companies", MatchPriority = 75 }
                    }
                },
                new
                {
                    AMCName = "Kotak Mahindra Mutual Fund",
                    Funds = new[]
                    {
                        new { FundName = "Kotak Bluechip Fund", Category = "Equity", SubCategory = "Large Cap", Notes = "Large cap fund with focus on bluechip companies", MatchPriority = 90 },
                        new { FundName = "Kotak Emerging Equity Fund", Category = "Equity", SubCategory = "Mid Cap", Notes = "Mid cap fund with emerging companies focus", MatchPriority = 85 },
                        new { FundName = "Kotak Small Cap Fund", Category = "Equity", SubCategory = "Small Cap", Notes = "Small cap fund with high growth potential", MatchPriority = 80 },
                        new { FundName = "Kotak Multicap Fund", Category = "Equity", SubCategory = "Multi Cap", Notes = "Multi cap fund with diversified portfolio", MatchPriority = 85 },
                        new { FundName = "Kotak Tax Saver Fund", Category = "Equity", SubCategory = "ELSS", Notes = "ELSS fund with tax benefits", MatchPriority = 85 },
                        new { FundName = "Kotak Banking & Financial Services Fund", Category = "Equity", SubCategory = "Sector", Notes = "Sector fund focusing on banking and financial services", MatchPriority = 75 }
                    }
                },
                new
                {
                    AMCName = "Motilal Oswal Mutual Fund",
                    Funds = new[]
                    {
                        new { FundName = "Motilal Oswal Large Cap Fund", Category = "Equity", SubCategory = "Large Cap", Notes = "Large cap fund with quality focus", MatchPriority = 90 },
                        new { FundName = "Motilal Oswal Midcap Fund", Category = "Equity", SubCategory = "Mid Cap", Notes = "Mid cap fund with growth companies", MatchPriority = 85 },
                        new { FundName = "Motilal Oswal Small Cap Fund", Category = "Equity", SubCategory = "Small Cap", Notes = "Small cap fund with high growth potential", MatchPriority = 80 },
                        new { FundName = "Motilal Oswal Multicap 35 Fund", Category = "Equity", SubCategory = "Multi Cap", Notes = "Multi cap fund with focused portfolio", MatchPriority = 85 },
                        new { FundName = "Motilal Oswal Long Term Equity Fund", Category = "Equity", SubCategory = "ELSS", Notes = "ELSS fund with tax benefits", MatchPriority = 85 },
                        new { FundName = "Motilal Oswal NASDAQ 100 ETF", Category = "Equity", SubCategory = "International", Notes = "International fund tracking NASDAQ 100", MatchPriority = 70 }
                    }
                },
                new
                {
                    AMCName = "Bandhan Mutual Fund",
                    Funds = new[]
                    {
                        new { FundName = "Bandhan Large Cap Fund", Category = "Equity", SubCategory = "Large Cap", Notes = "Large cap fund with focus on quality companies", MatchPriority = 90 },
                        new { FundName = "Bandhan Mid Cap Fund", Category = "Equity", SubCategory = "Mid Cap", Notes = "Mid cap fund with growth orientation", MatchPriority = 85 },
                        new { FundName = "Bandhan Small Cap Fund", Category = "Equity", SubCategory = "Small Cap", Notes = "Small cap fund with high growth potential", MatchPriority = 80 },
                        new { FundName = "Bandhan Multicap Fund", Category = "Equity", SubCategory = "Multi Cap", Notes = "Multi cap fund with diversified portfolio", MatchPriority = 85 },
                        new { FundName = "Bandhan ELSS Fund", Category = "Equity", SubCategory = "ELSS", Notes = "ELSS fund with tax benefits", MatchPriority = 85 },
                        new { FundName = "Bandhan Liquid Fund", Category = "Debt", SubCategory = "Liquid", Notes = "Liquid fund for short-term investments", MatchPriority = 85 }
                    }
                },
                new
                {
                    AMCName = "HSBC Mutual Fund",
                    Funds = new[]
                    {
                        new { FundName = "HSBC Large Cap Equity Fund", Category = "Equity", SubCategory = "Large Cap", Notes = "Large cap fund with global investment philosophy", MatchPriority = 90 },
                        new { FundName = "HSBC Mid Cap Equity Fund", Category = "Equity", SubCategory = "Mid Cap", Notes = "Mid cap fund with focus on growth companies", MatchPriority = 85 },
                        new { FundName = "HSBC Small Cap Equity Fund", Category = "Equity", SubCategory = "Small Cap", Notes = "Small cap fund with emerging companies focus", MatchPriority = 80 },
                        new { FundName = "HSBC Multi Cap Equity Fund", Category = "Equity", SubCategory = "Multi Cap", Notes = "Multi cap fund with flexible allocation", MatchPriority = 85 },
                        new { FundName = "HSBC Tax Saver Equity Fund", Category = "Equity", SubCategory = "ELSS", Notes = "ELSS fund with tax benefits", MatchPriority = 85 },
                        new { FundName = "HSBC Liquid Fund", Category = "Debt", SubCategory = "Liquid", Notes = "Liquid fund with high safety", MatchPriority = 85 }
                    }
                },
                new
                {
                    AMCName = "Quant Mutual Fund",
                    Funds = new[]
                    {
                        new { FundName = "Quant Large Cap Fund", Category = "Equity", SubCategory = "Large Cap", Notes = "Large cap fund with quantitative strategy", MatchPriority = 90 },
                        new { FundName = "Quant Mid Cap Fund", Category = "Equity", SubCategory = "Mid Cap", Notes = "Mid cap fund with quantitative approach", MatchPriority = 85 },
                        new { FundName = "Quant Small Cap Fund", Category = "Equity", SubCategory = "Small Cap", Notes = "Small cap fund with quantitative model", MatchPriority = 80 },
                        new { FundName = "Quant Multi Cap Fund", Category = "Equity", SubCategory = "Multi Cap", Notes = "Multi cap fund with quantitative allocation", MatchPriority = 85 },
                        new { FundName = "Quant ELSS Fund", Category = "Equity", SubCategory = "ELSS", Notes = "ELSS fund with quantitative strategy", MatchPriority = 85 },
                        new { FundName = "Quant Liquid Fund", Category = "Debt", SubCategory = "Liquid", Notes = "Liquid fund with quantitative management", MatchPriority = 85 }
                    }
                },
                new
                {
                    AMCName = "Groww Mutual Fund",
                    Funds = new[]
                    {
                        new { FundName = "Groww Large Cap Fund", Category = "Equity", SubCategory = "Large Cap", Notes = "Large cap fund with digital-first approach", MatchPriority = 90 },
                        new { FundName = "Groww Mid Cap Fund", Category = "Equity", SubCategory = "Mid Cap", Notes = "Mid cap fund with growth focus", MatchPriority = 85 },
                        new { FundName = "Groww Small Cap Fund", Category = "Equity", SubCategory = "Small Cap", Notes = "Small cap fund with high growth potential", MatchPriority = 80 },
                        new { FundName = "Groww Multicap Fund", Category = "Equity", SubCategory = "Multi Cap", Notes = "Multi cap fund with flexible allocation", MatchPriority = 85 },
                        new { FundName = "Groww ELSS Fund", Category = "Equity", SubCategory = "ELSS", Notes = "ELSS fund with tax benefits", MatchPriority = 85 },
                        new { FundName = "Groww Liquid Fund", Category = "Debt", SubCategory = "Liquid", Notes = "Liquid fund for short-term investments", MatchPriority = 85 }
                    }
                },
                new
                {
                    AMCName = "Zerodha Mutual Fund",
                    Funds = new[]
                    {
                        new { FundName = "Zerodha Large Cap Fund", Category = "Equity", SubCategory = "Large Cap", Notes = "Large cap fund with low-cost approach", MatchPriority = 90 },
                        new { FundName = "Zerodha Mid Cap Fund", Category = "Equity", SubCategory = "Mid Cap", Notes = "Mid cap fund with cost-effective strategy", MatchPriority = 85 },
                        new { FundName = "Zerodha Small Cap Fund", Category = "Equity", SubCategory = "Small Cap", Notes = "Small cap fund with high growth potential", MatchPriority = 80 },
                        new { FundName = "Zerodha Multicap Fund", Category = "Equity", SubCategory = "Multi Cap", Notes = "Multi cap fund with diversified portfolio", MatchPriority = 85 },
                        new { FundName = "Zerodha ELSS Fund", Category = "Equity", SubCategory = "ELSS", Notes = "ELSS fund with tax benefits", MatchPriority = 85 },
                        new { FundName = "Zerodha Liquid Fund", Category = "Debt", SubCategory = "Liquid", Notes = "Liquid fund with high safety", MatchPriority = 85 }
                    }
                },
                new
                {
                    AMCName = "Invesco Mutual Fund",
                    Funds = new[]
                    {
                        new { FundName = "Invesco India Large Cap Fund", Category = "Equity", SubCategory = "Large Cap", Notes = "Large cap fund with global expertise", MatchPriority = 90 },
                        new { FundName = "Invesco India Mid Cap Fund", Category = "Equity", SubCategory = "Mid Cap", Notes = "Mid cap fund with growth focus", MatchPriority = 85 },
                        new { FundName = "Invesco India Small Cap Fund", Category = "Equity", SubCategory = "Small Cap", Notes = "Small cap fund with emerging companies", MatchPriority = 80 },
                        new { FundName = "Invesco India Multicap Fund", Category = "Equity", SubCategory = "Multi Cap", Notes = "Multi cap fund with flexible allocation", MatchPriority = 85 },
                        new { FundName = "Invesco India ELSS Fund", Category = "Equity", SubCategory = "ELSS", Notes = "ELSS fund with tax benefits", MatchPriority = 85 },
                        new { FundName = "Invesco India Liquid Fund", Category = "Debt", SubCategory = "Liquid", Notes = "Liquid fund with high credit quality", MatchPriority = 85 }
                    }
                },
                new
                {
                    AMCName = "Edelweiss Mutual Fund",
                    Funds = new[]
                    {
                        new { FundName = "Edelweiss Large Cap Fund", Category = "Equity", SubCategory = "Large Cap", Notes = "Large cap fund with quality focus", MatchPriority = 90 },
                        new { FundName = "Edelweiss Mid Cap Fund", Category = "Equity", SubCategory = "Mid Cap", Notes = "Mid cap fund with growth orientation", MatchPriority = 85 },
                        new { FundName = "Edelweiss Small Cap Fund", Category = "Equity", SubCategory = "Small Cap", Notes = "Small cap fund with high growth potential", MatchPriority = 80 },
                        new { FundName = "Edelweiss Multicap Fund", Category = "Equity", SubCategory = "Multi Cap", Notes = "Multi cap fund with diversified portfolio", MatchPriority = 85 },
                        new { FundName = "Edelweiss ELSS Fund", Category = "Equity", SubCategory = "ELSS", Notes = "ELSS fund with tax benefits", MatchPriority = 85 },
                        new { FundName = "Edelweiss Liquid Fund", Category = "Debt", SubCategory = "Liquid", Notes = "Liquid fund for short-term investments", MatchPriority = 85 }
                    }
                },
                new
                {
                    AMCName = "Bank of India Mutual Fund",
                    Funds = new[]
                    {
                        new { FundName = "Bank of India Large Cap Fund", Category = "Equity", SubCategory = "Large Cap", Notes = "Large cap fund with conservative approach", MatchPriority = 90 },
                        new { FundName = "Bank of India Mid Cap Fund", Category = "Equity", SubCategory = "Mid Cap", Notes = "Mid cap fund with balanced strategy", MatchPriority = 85 },
                        new { FundName = "Bank of India Small Cap Fund", Category = "Equity", SubCategory = "Small Cap", Notes = "Small cap fund with growth potential", MatchPriority = 80 },
                        new { FundName = "Bank of India Multicap Fund", Category = "Equity", SubCategory = "Multi Cap", Notes = "Multi cap fund with flexible allocation", MatchPriority = 85 },
                        new { FundName = "Bank of India ELSS Fund", Category = "Equity", SubCategory = "ELSS", Notes = "ELSS fund with tax benefits", MatchPriority = 85 },
                        new { FundName = "Bank of India Liquid Fund", Category = "Debt", SubCategory = "Liquid", Notes = "Liquid fund with high safety", MatchPriority = 85 }
                    }
                },
                new
                {
                    AMCName = "Aditya Birla Mutual Fund",
                    Funds = new[]
                    {
                        new { FundName = "Aditya Birla Sun Life Large Cap Fund", Category = "Equity", SubCategory = "Large Cap", Notes = "Large cap fund with quality companies", MatchPriority = 90 },
                        new { FundName = "Aditya Birla Sun Life Mid Cap Fund", Category = "Equity", SubCategory = "Mid Cap", Notes = "Mid cap fund with growth focus", MatchPriority = 85 },
                        new { FundName = "Aditya Birla Sun Life Small Cap Fund", Category = "Equity", SubCategory = "Small Cap", Notes = "Small cap fund with high growth potential", MatchPriority = 80 },
                        new { FundName = "Aditya Birla Sun Life Multicap Fund", Category = "Equity", SubCategory = "Multi Cap", Notes = "Multi cap fund with diversified portfolio", MatchPriority = 85 },
                        new { FundName = "Aditya Birla Sun Life Tax Relief 96", Category = "Equity", SubCategory = "ELSS", Notes = "ELSS fund with tax benefits", MatchPriority = 85 },
                        new { FundName = "Aditya Birla Sun Life Liquid Fund", Category = "Debt", SubCategory = "Liquid", Notes = "Liquid fund with high credit quality", MatchPriority = 85 }
                    }
                },
                new
                {
                    AMCName = "DSP Mutual Fund",
                    Funds = new[]
                    {
                        new { FundName = "DSP Large Cap Fund", Category = "Equity", SubCategory = "Large Cap", Notes = "Large cap fund with quality focus", MatchPriority = 90 },
                        new { FundName = "DSP Mid Cap Fund", Category = "Equity", SubCategory = "Mid Cap", Notes = "Mid cap fund with growth orientation", MatchPriority = 85 },
                        new { FundName = "DSP Small Cap Fund", Category = "Equity", SubCategory = "Small Cap", Notes = "Small cap fund with high growth potential", MatchPriority = 80 },
                        new { FundName = "DSP Multicap Fund", Category = "Equity", SubCategory = "Multi Cap", Notes = "Multi cap fund with diversified portfolio", MatchPriority = 85 },
                        new { FundName = "DSP Tax Saver Fund", Category = "Equity", SubCategory = "ELSS", Notes = "ELSS fund with tax benefits", MatchPriority = 85 },
                        new { FundName = "DSP Liquid Fund", Category = "Debt", SubCategory = "Liquid", Notes = "Liquid fund for short-term investments", MatchPriority = 85 }
                    }
                },
                new
                {
                    AMCName = "Franklin Mutual Fund",
                    Funds = new[]
                    {
                        new { FundName = "Franklin India Large Cap Fund", Category = "Equity", SubCategory = "Large Cap", Notes = "Large cap fund with quality companies", MatchPriority = 90 },
                        new { FundName = "Franklin India Mid Cap Fund", Category = "Equity", SubCategory = "Mid Cap", Notes = "Mid cap fund with growth focus", MatchPriority = 85 },
                        new { FundName = "Franklin India Small Cap Fund", Category = "Equity", SubCategory = "Small Cap", Notes = "Small cap fund with high growth potential", MatchPriority = 80 },
                        new { FundName = "Franklin India Multicap Fund", Category = "Equity", SubCategory = "Multi Cap", Notes = "Multi cap fund with diversified portfolio", MatchPriority = 85 },
                        new { FundName = "Franklin India Taxshield Fund", Category = "Equity", SubCategory = "ELSS", Notes = "ELSS fund with tax benefits", MatchPriority = 85 },
                        new { FundName = "Franklin India Liquid Fund", Category = "Debt", SubCategory = "Liquid", Notes = "Liquid fund with high credit quality", MatchPriority = 85 }
                    }
                }
            };

            // Iterate through the structure and create ConcreteMutualFund entities
            foreach (var amc in amcFunds)
            {
                foreach (var fund in amc.Funds)
                {
                    databaseEntities.Add(new ConcreteMutualFund
                    {
                        Id = id++,
                        FundName = fund.FundName,
                        AMCName = amc.AMCName,
                        Category = fund.Category,
                        SubCategory = fund.SubCategory,
                        IsActive = true,
                        Notes = fund.Notes,
                        MatchPriority = fund.MatchPriority,
                        CreatedDate = DateTime.UtcNow,
                        LastUpdated = DateTime.UtcNow
                    });
                }
            }

            return databaseEntities;
        }
    }
        
    
}
