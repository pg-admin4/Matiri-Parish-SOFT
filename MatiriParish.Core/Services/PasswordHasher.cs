using System;
using System.Security.Cryptography;

namespace MatiriParish.Core.Services
{
    public static class PasswordHasher
    {
        public static (string Hash, string Salt, int Iterations, string Algorithm) HashPassword(string password, int iterations = 120000)
        {
            byte[] saltBytes = RandomNumberGenerator.GetBytes(16);
            using var pbkdf2 = new Rfc2898DeriveBytes(password, saltBytes, iterations, HashAlgorithmName.SHA256);
            byte[] hashBytes = pbkdf2.GetBytes(32);
            return (Convert.ToBase64String(hashBytes), Convert.ToBase64String(saltBytes), iterations, "PBKDF2-SHA256-v1");
        }

        public static bool VerifyPassword(string password, string storedHash, string storedSalt, int iterations)
        {
            if (string.IsNullOrEmpty(storedHash) || string.IsNullOrEmpty(storedSalt)) return false;
            byte[] saltBytes = Convert.FromBase64String(storedSalt);
            using var pbkdf2 = new Rfc2898DeriveBytes(password, saltBytes, iterations, HashAlgorithmName.SHA256);
            byte[] computed = pbkdf2.GetBytes(32);
            byte[] expected = Convert.FromBase64String(storedHash);
            return CryptographicOperations.FixedTimeEquals(computed, expected);
        }
    }
}
