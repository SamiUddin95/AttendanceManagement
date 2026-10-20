using System;
using System.Security.Cryptography;
using System.Text;

namespace AttendanceManagement.Infrastructure.Services.Security;

public static class PasswordHasher
{
    public static string Hash(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            throw new ArgumentException("Password cannot be empty", nameof(input));
        }

        var salt = RandomNumberGenerator.GetBytes(16);
        var bytes = Rfc2898DeriveBytes.Pbkdf2(input, salt, 600000, HashAlgorithmName.SHA256, 32);
        return $"PBKDF2$600000${Convert.ToBase64String(salt)}${Convert.ToBase64String(bytes)}";
    }

    public static bool Verify(string input, string hash)
    {
        if (string.IsNullOrWhiteSpace(hash) || string.IsNullOrEmpty(input))
        {
            return false;
        }

        byte[] existing;
        try
        {
            if (hash.StartsWith("PBKDF2$", StringComparison.Ordinal))
            {
                var parts = hash.Split('$');
                if (parts.Length != 4 || !int.TryParse(parts[1], out var iterations) || iterations < 100000 || iterations > 1000000) return false;
                var salt = Convert.FromBase64String(parts[2]);
                var expected = Convert.FromBase64String(parts[3]);
                if (salt.Length != 16 || expected.Length != 32) return false;
                return CryptographicOperations.FixedTimeEquals(expected,
                    Rfc2898DeriveBytes.Pbkdf2(input, salt, iterations, HashAlgorithmName.SHA256, 32));
            }
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
