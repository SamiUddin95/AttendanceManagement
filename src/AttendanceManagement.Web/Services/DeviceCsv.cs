using System.Globalization;
using Microsoft.VisualBasic.FileIO;

namespace AttendanceManagement.Web.Services;

public record DevicePunch(string EmployeeNo, DateTime Timestamp, string PunchType, string? DeviceId);
public static class DeviceCsv
{
    public static List<DevicePunch> Parse(TextReader reader)
    {
        using var parser = new TextFieldParser(reader) { HasFieldsEnclosedInQuotes = true, TrimWhiteSpace = true };
        parser.SetDelimiters(",");
        var header = parser.ReadFields();
        var expected = new[] { "EmployeeNo", "PunchDateTime", "PunchType", "DeviceId" };
        if (header == null || !header.SequenceEqual(expected, StringComparer.OrdinalIgnoreCase))
            throw new FormatException("Use columns EmployeeNo,PunchDateTime,PunchType,DeviceId in that order.");
        var rows = new List<DevicePunch>();
        while (!parser.EndOfData)
        {
            var line = parser.LineNumber;
            var fields = parser.ReadFields();
            if (fields == null || fields.Length != 4 || fields[0].Length is 0 or > 50 || fields[3].Length > 50 ||
                !DateTime.TryParseExact(fields[1], "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date) ||
                date.Year < 2000 || date > DateTime.Now.AddMinutes(5) ||
                !(fields[2].Equals("In", StringComparison.OrdinalIgnoreCase) || fields[2].Equals("Out", StringComparison.OrdinalIgnoreCase)))
                throw new FormatException($"Invalid record on line {line}. Use yyyy-MM-dd HH:mm:ss and In or Out; future punches are not accepted.");
            rows.Add(new(fields[0], DateTime.SpecifyKind(date, DateTimeKind.Unspecified), fields[2].Equals("In", StringComparison.OrdinalIgnoreCase) ? "In" : "Out", fields[3]));
            if (rows.Count > 10000) throw new FormatException("Import at most 10,000 punches at a time.");
        }
        if (rows.Count == 0) throw new FormatException("The file contains no punches.");
        return rows;
    }
}
