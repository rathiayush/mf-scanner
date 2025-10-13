# Background File Processing System

## Overview
The Background File Processing System allows bulk processing of Excel files containing mutual fund portfolio data. Users can dump multiple files in a directory, trigger processing with a single API call, and receive comprehensive reports showing which files and sheets passed or failed.

## 🎯 Key Features

### ✅ **Bulk File Processing**
- Process all Excel files in a directory automatically
- Support for both `.xlsx` and `.xls` formats
- One-click processing of hundreds of files

### 📊 **Sheet-Level Tracking** 
- Track success/failure for each sheet within files
- Detailed mutual fund detection results per sheet
- Holdings extraction and holding changes tracking per sheet

### 📈 **Real-Time Progress Monitoring**
- Live updates on processing status
- Current file being processed
- Success/failure counts in real-time

### 📋 **Comprehensive Excel Reporting**
- 4-sheet Excel report with detailed breakdown
- Summary statistics and processing metrics
- File-by-file and sheet-by-sheet results
- Complete error log with categorization

### 🔧 **Job Management**
- Start, monitor, cancel, and track multiple jobs
- Job history and status tracking
- Concurrent job execution support

## 📁 System Architecture

### **Files Created**

#### DTOs and Models
- `BackgroundProcessingDto.cs` - Request/response models
- `BackgroundJob.cs` - Database entity for job tracking
- `BackgroundJobStatus.cs` - Job state enumeration

#### Services
- `IBackgroundProcessingService.cs` - Service interface
- `BackgroundProcessingService.cs` - Core processing logic
- `IExcelReportService.cs` - Report generation interface
- `ExcelReportService.cs` - NPOI-based Excel report generator

#### API Controller
- `BackgroundProcessingController.cs` - REST endpoints

#### Database Integration
- Updated `MutualFundContext.cs` with BackgroundJob entity

## 🚀 API Endpoints

### **Job Management**
```http
POST /api/background-processing/start
GET /api/background-processing/status/{jobId}
GET /api/background-processing/jobs
POST /api/background-processing/cancel/{jobId}
```

### **Reporting**
```http
GET /api/background-processing/report/{jobId}
GET /api/background-processing/summary
```

### **Utilities**
```http
POST /api/background-processing/validate-directory
```

## 📊 Excel Report Structure

The generated Excel report contains 4 comprehensive sheets:

### 1. **Summary Sheet**
- Processing Information (directory, duration, status)
- File Statistics (total, successful, failed, success rate)
- Sheet Statistics (total processed, success rate)
- Business Statistics (funds created, holdings processed)

### 2. **File Results Sheet**
- File name and processing status
- Sheet counts (found, successful, failed)
- Processing duration per file
- Error messages for failed files

### 3. **Sheet Details Sheet**
- Individual sheet processing results
- Mutual fund detection information
- Holdings extracted and changes tracked
- Match confidence and fund creation status
- Processing duration per sheet

### 4. **Error Details Sheet**
- Comprehensive error log
- Error codes and categorization
- Processing stage where error occurred
- Detailed error messages and timestamps

## ⚙️ Configuration

### **appsettings.json**
```json
{
  "BackgroundProcessing": {
    "DefaultDirectory": "C:\\MutualFundFiles",
    "ReportsDirectory": "C:\\MutualFundReports"
  }
}
```

### **Service Registration (Program.cs)**
```csharp
builder.Services.AddScoped<IBackgroundProcessingService, BackgroundProcessingService>();
builder.Services.AddScoped<IExcelReportService, ExcelReportService>();
```

### **Database Migration**
```bash
dotnet ef migrations add AddBackgroundJobTable
dotnet ef database update
```

## 🔄 Usage Flow

### **1. Prepare Files**
```
C:\MutualFundFiles\
├── Fund_A_Jan_2024.xlsx
├── Fund_B_Jan_2024.xlsx
├── Fund_C_Jan_2024.xlsx
└── ...
```

### **2. Start Processing**
```http
POST /api/background-processing/start
Content-Type: application/json

{
  "directoryPath": "C:\\MutualFundFiles",
  "reportDate": "2024-01-31",
  "performDateCheck": false,
  "generateExcelReport": true,
  "filePatterns": ["*.xlsx", "*.xls"]
}
```

**Response:**
```json
{
  "jobId": "abc123-def456-ghi789",
  "message": "Background processing started successfully"
}
```

### **3. Monitor Progress**
```http
GET /api/background-processing/status/abc123-def456-ghi789
```

**Response:**
```json
{
  "jobId": "abc123-def456-ghi789",
  "status": "Running",
  "message": "Job is currently running",
  "startTime": "2024-01-31T10:00:00Z",
  "totalFiles": 50,
  "processedFiles": 25,
  "successfulFiles": 23,
  "failedFiles": 2,
  "currentFile": "Fund_X_Jan_2024.xlsx"
}
```

### **4. Download Report**
```http
GET /api/background-processing/report/abc123-def456-ghi789
```

Returns Excel file with comprehensive processing results.

## 🔧 Technical Implementation

### **Processing Logic**
1. **File Discovery**: Scan directory for Excel files matching patterns
2. **Sequential Processing**: Process files one by one using existing upload logic
3. **Progress Tracking**: Update database with real-time progress
4. **Error Handling**: Capture and categorize all errors
5. **Report Generation**: Create detailed Excel report using NPOI
6. **Job Completion**: Update final status and results

### **Database Schema**
```sql
CREATE TABLE BackgroundJobs (
    Id NVARCHAR(50) PRIMARY KEY,
    JobType NVARCHAR(50) NOT NULL,
    Status INT NOT NULL,
    DirectoryPath NVARCHAR(500),
    ReportDate DATETIME2 NOT NULL,
    PerformDateCheck BIT NOT NULL,
    FilePatterns NVARCHAR(1000),
    GenerateExcelReport BIT NOT NULL,
    TotalFiles INT NOT NULL DEFAULT 0,
    ProcessedFiles INT NOT NULL DEFAULT 0,
    SuccessfulFiles INT NOT NULL DEFAULT 0,
    FailedFiles INT NOT NULL DEFAULT 0,
    CurrentFile NVARCHAR(255),
    CreatedAt DATETIME2 NOT NULL,
    StartedAt DATETIME2,
    CompletedAt DATETIME2,
    ResultJson NVARCHAR(MAX),
    ExcelReportPath NVARCHAR(500),
    ErrorMessage NVARCHAR(2000),
    CreatedBy NVARCHAR(100) NOT NULL,
    Notes NVARCHAR(1000)
);
```

### **Error Handling Categories**
- **Validation Errors**: Date validation, file format issues
- **Detection Errors**: Mutual fund not detected, missing columns
- **Processing Errors**: Data extraction failures, database issues
- **System Errors**: Unexpected errors, service unavailable

### **Performance Considerations**
- **In-Memory Job Tracking**: Uses ConcurrentDictionary for active jobs
- **Database Transactions**: Proper Unit of Work pattern implementation
- **Cancellation Support**: Graceful job cancellation with cleanup
- **Resource Management**: Proper disposal of file streams and Excel workbooks

## 🎯 Business Value

### **Operational Efficiency**
- **Bulk Processing**: Process hundreds of files in one operation
- **Automated Reporting**: Comprehensive reports without manual intervention
- **Error Visibility**: Clear identification of processing issues
- **Progress Tracking**: Real-time visibility into processing status

### **Data Quality**
- **Sheet-Level Validation**: Identify specific sheets with issues
- **Mutual Fund Detection**: Track fund detection success rates
- **Holdings Accuracy**: Monitor holdings extraction quality
- **Error Categorization**: Systematic error classification for improvement

### **Audit Trail**
- **Complete Job History**: Track all processing operations
- **Detailed Logs**: Comprehensive error and success logging
- **Processing Metrics**: Business intelligence on data quality
- **Report Archive**: Downloadable reports for compliance

## 🚨 Troubleshooting

### **Common Issues**

#### **Job Stuck in Running State**
- Check application logs for errors
- Verify directory permissions
- Cancel and restart job if needed

#### **Files Not Processing**
- Validate directory path exists
- Check file permissions
- Verify file patterns match actual files

#### **Excel Report Not Generated**
- Check ReportsDirectory configuration
- Verify disk space availability
- Review ExcelReportService logs

#### **High Memory Usage**
- Monitor concurrent job count
- Check for large Excel files
- Consider processing in smaller batches

### **Monitoring Queries**
```sql
-- Active jobs
SELECT * FROM BackgroundJobs WHERE Status = 1; -- Running

-- Recent job summary
SELECT Status, COUNT(*) as Count 
FROM BackgroundJobs 
WHERE CreatedAt > DATEADD(day, -7, GETDATE())
GROUP BY Status;

-- Processing performance
SELECT 
    AVG(DATEDIFF(second, StartedAt, CompletedAt)) as AvgDurationSeconds,
    AVG(CAST(SuccessfulFiles as FLOAT) / TotalFiles * 100) as AvgSuccessRate
FROM BackgroundJobs 
WHERE Status = 2 AND CompletedAt IS NOT NULL; -- Completed
```

## 🔮 Future Enhancements

### **Planned Features**
- **Scheduled Processing**: Automatic processing on file system events
- **Email Notifications**: Send reports via email when jobs complete
- **Advanced Filtering**: Process only files matching specific criteria
- **Parallel Processing**: Process multiple files concurrently
- **Cloud Storage**: Support for Azure Blob Storage and AWS S3
- **API Rate Limiting**: Prevent system overload from concurrent requests

### **Integration Opportunities**
- **Power BI Integration**: Direct data export for business intelligence
- **Webhook Support**: Notify external systems on job completion
- **File Archiving**: Automatic archival of processed files
- **Data Validation Rules**: Configurable validation rules per fund type

---

## 📞 Support

For technical support or feature requests, please refer to the system logs and error details in the Excel reports. The comprehensive error categorization and detailed logging provide sufficient information for troubleshooting most issues.

**System Status Dashboard**: Use the `/api/background-processing/summary` endpoint for real-time system health monitoring.
