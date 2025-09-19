# Data Processing Strategy Memory Bank

## Excel File Processing Strategy

### Supported File Formats
- `.xlsx` (Excel 2007+)
- `.xls` (Legacy Excel)
- Multiple sheet support
- Various column naming conventions

### Column Mapping Strategy
Different mutual funds use different column names. The system normalizes these into a standard format:

#### Standard Schema
```csharp
public class PortfolioHolding
{
    public string ISIN { get; set; }
    public string Ticker { get; set; }
    public string SecurityName { get; set; }
    public decimal Quantity { get; set; }
    public decimal MarketValue { get; set; }
    public decimal Weight { get; set; }
    public DateTime AsOfDate { get; set; }
    public string MutualFundCode { get; set; }
}
```

#### Column Mapping Configurations
```json
{
  "mappings": [
    {
      "fundType": "HDFC",
      "columns": {
        "ISIN": ["ISIN", "ISIN_CODE", "Security_ISIN"],
        "Ticker": ["TICKER", "SYMBOL", "SCRIP_CODE"],
        "SecurityName": ["SECURITY_NAME", "COMPANY_NAME", "NAME"],
        "Quantity": ["QUANTITY", "QTY", "SHARES"],
        "MarketValue": ["MARKET_VALUE", "VALUE", "AMOUNT"],
        "Weight": ["WEIGHT", "PERCENTAGE", "WEIGHT_PCT"]
      }
    }
  ]
}
```

### File Processing Workflow
1. **File Detection**: FileSystemWatcher monitors folders
2. **File Validation**: Check format, size, accessibility
3. **Duplicate Check**: Hash-based and metadata comparison
4. **Excel Parsing**: Read sheets and identify data ranges
5. **Column Mapping**: Apply fund-specific mapping rules
6. **Data Validation**: Validate data types and business rules
7. **Normalization**: Convert to standard schema
8. **Database Insert**: Bulk insert with transaction support
9. **Logging**: Record processing results and metrics

### Error Handling Strategy
- **File Level**: Skip corrupted files, log errors
- **Row Level**: Skip invalid rows, continue processing
- **Batch Processing**: Process in chunks to handle large files
- **Retry Logic**: Retry failed operations with exponential backoff
