using Microsoft.EntityFrameworkCore;
using MutualFundScanner.Domain.Entities;
using MutualFundScanner.Infrastructure.Data;

namespace MutualFundScanner.Infrastructure.Repositories;

public interface IPortfolioRepository
{
    Task<PortfolioFile?> GetFileByHashAsync(string hash);
    Task<List<PortfolioFile>> GetPendingFilesAsync();
    Task<bool> IsDuplicateFileAsync(string hash, string fileName);
    Task<PortfolioFile> AddFileAsync(PortfolioFile file);
    Task UpdateFileAsync(PortfolioFile file);
    Task BulkInsertHoldingsAsync(List<PortfolioHolding> holdings);
    Task<List<MutualFund>> GetActiveFundsAsync();
    Task<MutualFund?> GetFundByCodeAsync(string fundCode);
}

public class PortfolioRepository : IPortfolioRepository
{
    private readonly MutualFundContext _context;

    public PortfolioRepository(MutualFundContext context)
    {
        _context = context;
    }

    public async Task<PortfolioFile?> GetFileByHashAsync(string hash)
    {
        return await _context.PortfolioFiles
            .FirstOrDefaultAsync(f => f.FileHash == hash);
    }

    public async Task<List<PortfolioFile>> GetPendingFilesAsync()
    {
        return await _context.PortfolioFiles
            .Where(f => f.Status == Domain.Enums.ProcessingStatus.Pending)
            .Include(f => f.MutualFund)
            .OrderBy(f => f.CreatedDate)
            .ToListAsync();
    }

    public async Task<bool> IsDuplicateFileAsync(string hash, string fileName)
    {
        return await _context.PortfolioFiles
            .AnyAsync(f => f.FileHash == hash || f.FileName == fileName);
    }

    public async Task<PortfolioFile> AddFileAsync(PortfolioFile file)
    {
        _context.PortfolioFiles.Add(file);
        await _context.SaveChangesAsync();
        return file;
    }

    public async Task UpdateFileAsync(PortfolioFile file)
    {
        _context.PortfolioFiles.Update(file);
        await _context.SaveChangesAsync();
    }

    public async Task BulkInsertHoldingsAsync(List<PortfolioHolding> holdings)
    {
        _context.PortfolioHoldings.AddRange(holdings);
        await _context.SaveChangesAsync();
    }

    public async Task<List<MutualFund>> GetActiveFundsAsync()
    {
        return await _context.MutualFunds
            .Where(f => f.IsActive)
            .ToListAsync();
    }

    public async Task<MutualFund?> GetFundByCodeAsync(string fundCode)
    {
        return await _context.MutualFunds
            .FirstOrDefaultAsync(f => f.FundCode == fundCode);
    }
}
