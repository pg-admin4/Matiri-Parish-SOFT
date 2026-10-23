using System;
using MatiriParish.Core.Services;
using Xunit;

namespace MauiApp1.Tests
{
    public class PasswordHasherTests
    {
        [Fact]
        public void HashAndVerify_Password_IsVerified()
        {
            string password = "Test@1234";
            var result = PasswordHasher.HashPassword(password, 120000);
            // tuple returned as (hash, salt, iterations, algorithm) - access by ItemN to avoid deconstruction issues
            string hash = result.Item1;
            string salt = result.Item2;
            int iters = (int)result.Item3;
            string alg = result.Item4?.ToString() ?? string.Empty;

            Assert.False(string.IsNullOrEmpty(hash));
            Assert.False(string.IsNullOrEmpty(salt));
            Assert.True(iters >= 120000);
            Assert.Equal("PBKDF2-SHA256-v1", alg);

            bool ok = PasswordHasher.VerifyPassword(password, hash, salt, iters);
            Assert.True(ok);

            bool wrong = PasswordHasher.VerifyPassword(password + "x", hash, salt, iters);
            Assert.False(wrong);
        }
    }
}
