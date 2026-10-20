namespace AttendanceManagement.Web.Security;

public static class EmployeePassword
{
    public static bool Verify(string password, string? hash)
    {
        if (string.IsNullOrWhiteSpace(hash) || string.IsNullOrEmpty(password)) return false;
        try { return BCrypt.Net.BCrypt.Verify(password, hash); }
        catch (BCrypt.Net.SaltParseException) { return false; }
        catch (ArgumentException) { return false; }
    }
}
