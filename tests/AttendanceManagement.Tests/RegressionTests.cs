using System.Security.Cryptography;
using System.Text;
using AttendanceManagement.Domain.Entities;
using AttendanceManagement.Infrastructure.Services.Security;
using AttendanceManagement.Web.Services;
using Xunit;

public class RegressionTests
{
    [Fact] public void DeviceCsvParsesQuotedEmployeeNumbersAndOvernightPunches()
    {
        var rows = DeviceCsv.Parse(new StringReader("EmployeeNo,PunchDateTime,PunchType,DeviceId\n\"DEMO-001\",2026-01-01 20:00:00,in,D1\nDEMO-001,2026-01-02 04:00:00,Out,D1"));
        Assert.Equal(2, rows.Count); Assert.Equal("In", rows[0].PunchType);
    }
    [Theory]
    [InlineData("EmployeeNo,Time,Direction,Device")]
    [InlineData("EmployeeNo,PunchDateTime,PunchType,DeviceId\nEMP,not-a-date,In,D1")]
    [InlineData("EmployeeNo,PunchDateTime,PunchType,DeviceId\nEMP,2026-01-01 08:00:00,Unknown,D1")]
    [InlineData("EmployeeNo,PunchDateTime,PunchType,DeviceId")]
    public void InvalidDeviceFilesAreRejected(string csv) => Assert.Throws<FormatException>(() => DeviceCsv.Parse(new StringReader(csv)));
    [Fact] public void PasswordHashesAreSaltedAndVerify()
    {
        var hash = PasswordHasher.Hash("Hospital-Test!42");
        Assert.NotEqual(hash, PasswordHasher.Hash("Hospital-Test!42"));
        Assert.True(PasswordHasher.Verify("Hospital-Test!42", hash));
        Assert.False(PasswordHasher.Verify("wrong", hash));
    }
    [Theory]
    [InlineData("")][InlineData("bad")][InlineData("PBKDF2$9999999999$AA$BB")][InlineData("PBKDF2$600000$bad$bad")]
    public void MalformedHashesFailClosed(string hash) => Assert.False(PasswordHasher.Verify("test", hash));
    [Fact] public void LegacyPasswordsStillVerifyForMigration()
    {
        var legacy = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes("old-password")));
        Assert.True(PasswordHasher.Verify("old-password", legacy));
    }
    static Attendance Punch(DateTime date, string type) => new() { PunchDateTime = date, PunchType = type };
    [Fact] public void OvernightShiftIsOnePresentRow()
    {
        var start = new DateTime(2026, 9, 30, 20, 0, 0);
        var row = Assert.Single(PunchPairing.Build([Punch(start, "In"), Punch(start.AddHours(8), "Out")]));
        Assert.Equal(start.Date, row.Date); Assert.Equal("Present", row.Status); Assert.Null(row.ExtraHours);
        Assert.Equal(TimeSpan.FromHours(4), row.TimeOut);
    }
    [Fact] public void RepeatedEntriesAreNotSilentlyLost()
    {
        var start = new DateTime(2026, 10, 1, 8, 0, 0);
        var rows = PunchPairing.Build([Punch(start, "In"), Punch(start.AddDays(1), "In"), Punch(start.AddDays(1).AddHours(8), "Out")]);
        Assert.Equal(2, rows.Count); Assert.Equal("Half Day (No Out)", rows[0].Status); Assert.Equal("Present", rows[1].Status);
    }
    [Fact] public void StaleEntryCannotPairWithNextWeeksExit()
    {
        var start = new DateTime(2026, 10, 1, 8, 0, 0);
        var rows = PunchPairing.Build([Punch(start, "In"), Punch(start.AddDays(7), "Out")]);
        Assert.Equal(2, rows.Count); Assert.Equal("Half Day (No In)", rows[1].Status);
    }
    [Fact] public void OvertimeUsesFullDateTimes()
    {
        var start = new DateTime(2026, 10, 1, 20, 0, 0);
        var row = Assert.Single(PunchPairing.Build([Punch(start, "In"), Punch(start.AddHours(10), "Out")]));
        Assert.Equal(TimeSpan.FromHours(2), row.ExtraHours);
    }
}
