# Backend Strategy Memory Bank

## .NET Core API Implementation Strategy

### Project Structure
```
backend/
├── MutualFundScanner.API/          # Web API layer
├── MutualFundScanner.Application/  # Application services
├── MutualFundScanner.Domain/       # Domain models
├── MutualFundScanner.Infrastructure/ # Data access & external services
└── MutualFundScanner.Tests/        # Unit and integration tests
```

### Clean Architecture Layers

#### Domain Layer
```csharp
// Core business entities
public class MutualFund
{
    public int Id { get; set; }
    public string FundCode { get; set; }
    public string FundName { get; set; }
    public string FolderPath { get; set; }
    public List<PortfolioFile> Files { get; set; }
}

public class PortfolioFile
{
    public int Id { get; set; }
    public string FileName { get; set; }
    public string FilePath { get; set; }
    public string FileHash { get; set; }
    public DateTime ProcessedDate { get; set; }
    public ProcessingStatus Status { get; set; }
    public List<PortfolioHolding> Holdings { get; set; }
}
```

#### Application Layer
```csharp
// CQRS Commands and Queries
public class ProcessFileCommand : IRequest<ProcessFileResult>
{
    public string FilePath { get; set; }
    public string MutualFundCode { get; set; }
}

public class GetProcessingStatusQuery : IRequest<ProcessingStatusDto>
{
    public int FileId { get; set; }
}
```

### Background Services
```csharp
// File monitoring service
public class FileWatcherService : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Monitor folders for new files
        // Queue files for processing
        // Handle file system events
    }
}

// File processing service
public class FileProcessingService : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Process queued files
        // Handle Excel parsing
        // Update database
    }
}
```

### Excel Processing Strategy
```csharp
public interface IExcelProcessor
{
    Task<ProcessingResult> ProcessFileAsync(string filePath, MutualFund fund);
}

public class ExcelProcessor : IExcelProcessor
{
    // EPPlus for Excel processing
    // Column mapping logic
    // Data validation
    // Bulk insert operations
}
```

### API Endpoints
- `GET /api/funds` - List all mutual funds
- `POST /api/funds` - Add new mutual fund
- `GET /api/files` - List processed files
- `GET /api/files/{id}/status` - Get processing status
- `POST /api/files/reprocess` - Reprocess failed files
- `GET /api/holdings` - Get portfolio holdings
- `GET /api/logs` - Get processing logs
