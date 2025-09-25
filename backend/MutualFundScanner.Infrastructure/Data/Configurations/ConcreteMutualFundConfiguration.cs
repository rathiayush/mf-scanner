using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MutualFundScanner.Domain.Entities;

namespace MutualFundScanner.Infrastructure.Data.Configurations;

public class ConcreteMutualFundConfiguration : IEntityTypeConfiguration<ConcreteMutualFund>
{
    public void Configure(EntityTypeBuilder<ConcreteMutualFund> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.FundName).HasMaxLength(500).IsRequired();
        builder.Property(e => e.AMCName).HasMaxLength(100);
        builder.Property(e => e.Category).HasMaxLength(100);
        builder.Property(e => e.SubCategory).HasMaxLength(100);
        builder.Property(e => e.Notes).HasMaxLength(1000);
        builder.HasIndex(e => e.FundName);
        builder.HasIndex(e => new { e.AMCName, e.Category });
        builder.HasIndex(e => e.MatchPriority);
    }
}
