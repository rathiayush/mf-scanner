using Microsoft.EntityFrameworkCore;
using MutualFundScanner.Domain.Entities;
using MutualFundScanner.Domain.Enums;
using MutualFundScanner.Infrastructure.Data.Configurations;

namespace MutualFundScanner.Infrastructure.Data;

public class MutualFundContext : DbContext
{
    public MutualFundContext(DbContextOptions<MutualFundContext> options) : base(options)
    {
    }

    public DbSet<MutualFund> MutualFunds { get; set; }
    public DbSet<PortfolioFile> PortfolioFiles { get; set; }
    public DbSet<PortfolioHolding> PortfolioHoldings { get; set; }
    public DbSet<ProcessingLog> ProcessingLogs { get; set; }
    public DbSet<PortfolioSnapshot> PortfolioSnapshots { get; set; }
    public DbSet<HoldingChange> HoldingChanges { get; set; }
    public DbSet<ConcreteMutualFund> ConcreteMutualFunds { get; set; }
    public DbSet<BackgroundJob> BackgroundJobs { get; set; }
    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // MutualFund configuration
        modelBuilder.Entity<MutualFund>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FundCode).HasMaxLength(50).IsRequired();
            entity.Property(e => e.FundName).HasMaxLength(200).IsRequired();
            entity.Property(e => e.FolderPath).HasMaxLength(500).IsRequired();
            entity.HasIndex(e => e.FundCode).IsUnique();
        });

        // PortfolioFile configuration
        modelBuilder.Entity<PortfolioFile>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FileName).HasMaxLength(255).IsRequired();
            entity.Property(e => e.FilePath).HasMaxLength(500).IsRequired();
            entity.Property(e => e.FileHash).HasMaxLength(64).IsRequired();
            entity.HasIndex(e => e.FileHash).IsUnique();
            entity.HasOne(e => e.MutualFund)
                  .WithMany(e => e.Files)
                  .HasForeignKey(e => e.MutualFundId)
                  .OnDelete(DeleteBehavior.NoAction);
        });

        // PortfolioHolding configuration
        modelBuilder.Entity<PortfolioHolding>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ISIN).HasMaxLength(12);
            entity.Property(e => e.Ticker).HasMaxLength(50);
            entity.Property(e => e.SecurityName).HasMaxLength(200);
            entity.Property(e => e.Sector).HasMaxLength(100);
            entity.Property(e => e.Industry).HasMaxLength(100);
            entity.Property(e => e.Quantity).HasPrecision(18, 4);
            entity.Property(e => e.MarketValue).HasPrecision(18, 2);
            entity.Property(e => e.Weight).HasPrecision(5, 4);
            entity.Property(e => e.PreviousWeight).HasPrecision(5, 4);
            entity.HasIndex(e => new { e.PortfolioFileId, e.ISIN, e.AsOfDate });
            entity.HasOne(e => e.PortfolioFile)
                  .WithMany(e => e.Holdings)
                  .HasForeignKey(e => e.PortfolioFileId)
                  .OnDelete(DeleteBehavior.NoAction);
        });

        // ProcessingLog configuration
        modelBuilder.Entity<ProcessingLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Message).IsRequired();
            entity.HasOne(e => e.PortfolioFile)
                  .WithMany(e => e.Logs)
                  .HasForeignKey(e => e.PortfolioFileId)
                  .OnDelete(DeleteBehavior.NoAction);
        });

        // PortfolioSnapshot configuration
        modelBuilder.Entity<PortfolioSnapshot>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TotalValue).HasPrecision(18, 2);
            entity.HasIndex(e => new { e.MutualFundId, e.SnapshotDate }).IsUnique();
            entity.HasOne(e => e.MutualFund)
                  .WithMany(e => e.Snapshots)
                  .HasForeignKey(e => e.MutualFundId)
                  .OnDelete(DeleteBehavior.NoAction);
        });

        // HoldingChange configuration
        modelBuilder.Entity<HoldingChange>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ISIN).HasMaxLength(12);
            entity.Property(e => e.Ticker).HasMaxLength(50);
            entity.Property(e => e.SecurityName).HasMaxLength(200);
            entity.Property(e => e.PreviousQuantity).HasPrecision(18, 4);
            entity.Property(e => e.NewQuantity).HasPrecision(18, 4);
            entity.Property(e => e.PreviousWeight).HasPrecision(5, 4);
            entity.Property(e => e.NewWeight).HasPrecision(5, 4);
            entity.Property(e => e.PreviousValue).HasPrecision(18, 2);
            entity.Property(e => e.NewValue).HasPrecision(18, 2);
            entity.HasIndex(e => new { e.MutualFundId, e.ISIN, e.FromDate, e.ToDate });
            entity.HasOne(e => e.MutualFund)
                  .WithMany(e => e.HoldingChanges)
                  .HasForeignKey(e => e.MutualFundId)
                  .OnDelete(DeleteBehavior.NoAction);
        });

        // BackgroundJob configuration
        modelBuilder.Entity<BackgroundJob>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasMaxLength(50);
            entity.Property(e => e.JobType).HasMaxLength(50).IsRequired();
            entity.Property(e => e.DirectoryPath).HasMaxLength(500);
            entity.Property(e => e.FilePatterns).HasMaxLength(1000);
            entity.Property(e => e.CurrentFile).HasMaxLength(255);
            entity.Property(e => e.ExcelReportPath).HasMaxLength(500);
            entity.Property(e => e.ErrorMessage).HasMaxLength(2000);
            entity.Property(e => e.CreatedBy).HasMaxLength(100);
            entity.Property(e => e.Notes).HasMaxLength(1000);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.CreatedAt);
        });

        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Username).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Email).HasMaxLength(255).IsRequired();
            entity.Property(e => e.PasswordHash).IsRequired();
            entity.Property(e => e.Notes).HasMaxLength(500);
            entity.HasIndex(e => e.Username).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasOne(e => e.CreatedBy)
                  .WithMany()
                  .HasForeignKey(e => e.CreatedByUserId)
                  .OnDelete(DeleteBehavior.NoAction);
        });

        // Apply entity configurations
        modelBuilder.ApplyConfiguration(new ConcreteMutualFundConfiguration());
        // Seed configuration removed - using SQL scripts approach instead
    }
}
