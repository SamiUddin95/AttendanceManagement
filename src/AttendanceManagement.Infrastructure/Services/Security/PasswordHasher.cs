using System;
using System.Security.Cryptography;
using System.Text;

namespace AttendanceManagement.Infrastructure.Services.Security;

internal static class PasswordHasher
{
    public static string Hash(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            throw new ArgumentException("Password cannot be empty", nameof(input));
        }

        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes);
    }

    public static bool Verify(string input, string hash)
    {
        if (string.IsNullOrWhiteSpace(hash))
        {
            return false;
        }

        byte[] existing;
        try
        {
            existing = Convert.FromHexString(hash);
        }
        catch (FormatException)
        {
            return false;
        }

        var computed = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return CryptographicOperations.FixedTimeEquals(computed, existing);
    }
}
