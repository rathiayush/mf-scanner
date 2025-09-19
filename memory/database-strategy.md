# Database Strategy Memory Bank

## Entity Framework Core Implementation

### Database Schema Design

#### Core Tables
```sql
-- Mutual Funds table
CREATE TABLE MutualFunds (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    FundCode NVARCHAR(50) UNIQUE NOT NULL,
    FundName NVARCHAR(200) NOT NULL,
    FolderPath NVARCHAR(500) NOT NULL,
    IsActive BIT DEFAULT 1,
    CreatedDate DATETIME2 DEFAULT GETDATE(),
    LastScanDate DATETIME2
);

-- Portfolio Files table
CREATE TABLE PortfolioFiles (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    MutualFundId INT FOREIGN KEY REFERENCES MutualFunds(Id),
    FileName NVARCHAR(255) NOT NULL,
    FilePath NVARCHAR(500) NOT NULL,
    FileHash NVARCHAR(64) NOT NULL, -- SHA256 hash
    FileSize BIGINT NOT NULL,
    ProcessedDate DATETIME2,
    Status NVARCHAR(50) NOT NULL, -- Pending, Processing, Completed, Failed
    ErrorMessage NVARCHAR(MAX),
    RecordCount INT DEFAULT 0,
    CreatedDate DATETIME2 DEFAULT GETDATE()
);

-- Portfolio Holdings table
CREATE TABLE PortfolioHoldings (
    Id BIGINT IDENTITY(1,1) PRIMARY KEY,
    PortfolioFileId INT FOREIGN KEY REFERENCES PortfolioFiles(Id),
    ISIN NVARCHAR(12),
    Ticker NVARCHAR(50),
    SecurityName NVARCHAR(200),
    Quantity DECIMAL(18,4),
    MarketValue DECIMAL(18,2),
    Weight DECIMAL(5,4), -- Percentage as decimal
    AsOfDate DATE,
    CreatedDate DATETIME2 DEFAULT GETDATE()
);

-- Processing Logs table
CREATE TABLE ProcessingLogs (
    Id BIGINT IDENTITY(1,1) PRIMARY KEY,
    PortfolioFileId INT FOREIGN KEY REFERENCES PortfolioFiles(Id),
    LogLevel NVARCHAR(20) NOT NULL, -- Info, Warning, Error
    Message NVARCHAR(MAX) NOT NULL,
    Exception NVARCHAR(MAX),
    Timestamp DATETIME2 DEFAULT GETDATE()
);
```

### Entity Framework Models
```csharp
public class MutualFundContext : DbContext
{
    public DbSet<MutualFund> MutualFunds { get; set; }
    public DbSet<PortfolioFile> PortfolioFiles { get; set; }
    public DbSet<PortfolioHolding> PortfolioHoldings { get; set; }
    public DbSet<ProcessingLog> ProcessingLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure relationships and constraints
        modelBuilder.Entity<PortfolioFile>()
            .HasIndex(p => p.FileHash)
            .IsUnique();
            
        modelBuilder.Entity<PortfolioHolding>()
            .HasIndex(p => new { p.PortfolioFileId, p.ISIN, p.AsOfDate });
    }
}
```

### Repository Pattern
```csharp
public interface IPortfolioRepository
{
    Task<PortfolioFile> GetFileByHashAsync(string hash);
    Task<List<PortfolioFile>> GetPendingFilesAsync();
    Task<bool> IsDuplicateFileAsync(string hash, string fileName);
    Task BulkInsertHoldingsAsync(List<PortfolioHolding> holdings);
}
```

### Performance Optimizations
- **Indexing**: Strategic indexes on frequently queried columns
- **Bulk Operations**: Use EF Core bulk extensions for large datasets
- **Connection Pooling**: Configure connection pool for high throughput
- **Query Optimization**: Use compiled queries for repeated operations
- **Partitioning**: Consider table partitioning for large historical data
