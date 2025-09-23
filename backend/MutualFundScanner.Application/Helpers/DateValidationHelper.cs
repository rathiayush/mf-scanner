using System.Text.RegularExpressions;

namespace MutualFundScanner.Application.Helpers;

/// <summary>
/// Helper class for date validation operations including filename date extraction and consistency validation
/// </summary>
public class DateValidationHelper
{
    /// <summary>
    /// Validates that the reported date matches the date in the filename
    /// </summary>
    public static (bool IsValid, string ErrorMessage) ValidateDateConsistency(string fileName, DateTime? reportedDate)
    {
        if (!reportedDate.HasValue)
        {
            return (false, "Reported date is required for file upload validation");
        }

        // Extract date from filename using various common patterns
        var fileNameDate = ExtractDateFromFileName(fileName);
        
        if (!fileNameDate.HasValue)
        {
            return (false, $"Could not extract date from filename '{fileName}'. Please ensure filename contains date in format dd/MM/yyyy, dd-MM-yyyy, or ddMMyyyy");
        }

        // Compare dates (ignoring time component)
        var reportedDateOnly = reportedDate.Value.Date;
        var fileNameDateOnly = fileNameDate.Value.Date;

        if (reportedDateOnly != fileNameDateOnly)
        {
            return (false, $"Date mismatch: Reported date ({reportedDateOnly:dd/MM/yyyy}) does not match filename date ({fileNameDateOnly:dd/MM/yyyy})");
        }

        return (true, string.Empty);
    }

    /// <summary>
    /// Extracts date from filename using various common patterns
    /// </summary>
    public static DateTime? ExtractDateFromFileName(string fileName)
    {
        // Remove file extension
        var nameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
        
        // Common date patterns to try
        var patterns = new[]
        {
            @"(\d{1,2})[\/\-](\d{1,2})[\/\-](\d{4})", // dd/MM/yyyy or dd-MM-yyyy
            @"(\d{1,2})[\/\-](\d{1,2})[\/\-](\d{2})", // dd/MM/yy or dd-MM-yy
            @"(\d{2})(\d{2})(\d{4})", // ddMMyyyy
            @"(\d{4})[\/\-](\d{1,2})[\/\-](\d{1,2})", // yyyy/MM/dd or yyyy-MM-dd
            @"(\d{4})(\d{2})(\d{2})" // yyyyMMdd
        };

        foreach (var pattern in patterns)
        {
            var match = Regex.Match(nameWithoutExtension, pattern);
            if (match.Success)
            {
                try
                {
                    int day, month, year;
                    
                    if (pattern.Contains(@"(\d{4})[\/\-](\d{1,2})[\/\-](\d{1,2})") || pattern.Contains(@"(\d{4})(\d{2})(\d{2})"))
                    {
                        // yyyy/MM/dd or yyyyMMdd format
                        year = int.Parse(match.Groups[1].Value);
                        month = int.Parse(match.Groups[2].Value);
                        day = int.Parse(match.Groups[3].Value);
                    }
                    else
                    {
                        // dd/MM/yyyy or ddMMyyyy format
                        day = int.Parse(match.Groups[1].Value);
                        month = int.Parse(match.Groups[2].Value);
                        year = int.Parse(match.Groups[3].Value);
                        
                        // Handle 2-digit years
                        if (year < 100)
                        {
                            year += year < 50 ? 2000 : 1900;
                        }
                    }

                    return new DateTime(year, month, day);
                }
                catch
                {
                    // Continue to next pattern if parsing fails
                    continue;
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Validates that a date string can be parsed to a valid DateTime
    /// </summary>
    public static (bool IsValid, DateTime? ParsedDate, string ErrorMessage) ValidateAndParseDate(string dateString, string fieldName = "Date")
    {
        if (string.IsNullOrEmpty(dateString))
        {
            return (false, null, $"{fieldName} is required");
        }

        if (DateTime.TryParse(dateString, out var parsedDate))
        {
            return (true, parsedDate, string.Empty);
        }

        return (false, null, $"Invalid {fieldName} format. Please use a valid date format");
    }

    /// <summary>
    /// Validates that a date range is valid (from date is before or equal to to date)
    /// </summary>
    public static (bool IsValid, string ErrorMessage) ValidateDateRange(DateTime fromDate, DateTime toDate)
    {
        if (fromDate > toDate)
        {
            return (false, "From date cannot be later than To date");
        }

        return (true, string.Empty);
    }

    /// <summary>
    /// Validates that a date is not in the future
    /// </summary>
    public static (bool IsValid, string ErrorMessage) ValidateNotFutureDate(DateTime date, string fieldName = "Date")
    {
        if (date.Date > DateTime.Today)
        {
            return (false, $"{fieldName} cannot be in the future");
        }

        return (true, string.Empty);
    }
}
