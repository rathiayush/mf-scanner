# Logging Strategy Memory Bank

## Comprehensive Logging Implementation

### Logging Framework
- **Serilog**: Structured logging for .NET Core
- **Log Levels**: Verbose, Debug, Information, Warning, Error, Fatal
- **Sinks**: File, Console, Database, Application Insights

### Logging Configuration
```csharp
public static class LoggingConfiguration
{
    public static void ConfigureLogging(this IServiceCollection services, IConfiguration configuration)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithThreadId()
            .WriteTo.Console()
            .WriteTo.File("logs/mf-scanner-.txt", rollingInterval: RollingInterval.Day)
            .WriteTo.MSSqlServer(
                connectionString: configuration.GetConnectionString("DefaultConnection"),
                sinkOptions: new MSSqlServerSinkOptions { TableName = "ProcessingLogs" })
            .CreateLogger();
    }
}
```

### File Processing Audit Trail
```csharp
public class FileProcessingLogger
{
    private readonly ILogger<FileProcessingLogger> _logger;
    
    public void LogFileDetected(string filePath, string fundCode)
    {
        _logger.LogInformation("File detected: {FilePath} for fund {FundCode}", 
            filePath, fundCode);
    }
    
    public void LogProcessingStarted(int fileId, string fileName)
    {
        _logger.LogInformation("Processing started for file {FileId}: {FileName}", 
            fileId, fileName);
    }
    
    public void LogProcessingCompleted(int fileId, int recordCount, TimeSpan duration)
    {
        _logger.LogInformation("Processing completed for file {FileId}. " +
            "Records: {RecordCount}, Duration: {Duration}ms", 
            fileId, recordCount, duration.TotalMilliseconds);
    }
    
    public void LogProcessingError(int fileId, Exception exception)
    {
        _logger.LogError(exception, "Processing failed for file {FileId}", fileId);
    }
}
```

### Performance Monitoring
```csharp
public class PerformanceMetrics
{
    public void LogFileProcessingMetrics(FileProcessingMetrics metrics)
    {
        _logger.LogInformation("File Processing Metrics: " +
            "FileSize: {FileSize}MB, " +
            "ProcessingTime: {ProcessingTime}ms, " +
            "RecordsPerSecond: {RecordsPerSecond}, " +
            "MemoryUsage: {MemoryUsage}MB",
            metrics.FileSizeMB,
            metrics.ProcessingTimeMs,
            metrics.RecordsPerSecond,
            metrics.MemoryUsageMB);
    }
}
```

### Error Categorization
- **File Access Errors**: Permission, file locked, corrupted
- **Data Validation Errors**: Invalid formats, missing required fields
- **Database Errors**: Connection issues, constraint violations
- **Business Logic Errors**: Duplicate detection, mapping failures

### Log Analysis and Alerting
- **Error Rate Monitoring**: Track error percentage over time
- **Performance Degradation**: Alert on slow processing times
- **File Processing Failures**: Immediate alerts for critical failures
- **System Health**: Monitor memory usage, disk space, database connections
