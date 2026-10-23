using System;
using System.Security.Cryptography;

namespace MauiApp1.Services;

public static class PasswordHasher
{
    public const int DefaultIterations = 120_000;
    public const int SaltSize = 16; // bytes
    public const int HashSize = 32; // bytes
    public const string AlgorithmName = "PBKDF2-SHA256-v1";

    public static (string HashBase64, string SaltBase64, int Iterations, string Algorithm) HashPassword(string password, int? iterations = null)
    {
        if (password is null) throw new ArgumentNullException(nameof(password));

        int iters = iterations ?? DefaultIterations;
        byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);

        using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iters, HashAlgorithmName.SHA256);
        byte[] hash = pbkdf2.GetBytes(HashSize);

        return (Convert.ToBase64String(hash), Convert.ToBase64String(salt), iters, AlgorithmName);
    }

    public static bool VerifyPassword(string password, string storedHashBase64, string storedSaltBase64, int iterations)
    {
        if (password is null) return false;
        if (string.IsNullOrEmpty(storedHashBase64) || string.IsNullOrEmpty(storedSaltBase64) || iterations <= 0) return false;

        byte[] salt;
        byte[] storedHash;
        try
        {
            salt = Convert.FromBase64String(storedSaltBase64);
            storedHash = Convert.FromBase64String(storedHashBase64);
        }
        catch
        {
            return false;
        }

        using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256);
        byte[] computed = pbkdf2.GetBytes(HashSize);

        return CryptographicOperations.FixedTimeEquals(computed, storedHash);
    }
}
