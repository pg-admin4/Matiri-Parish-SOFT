using System;
using Xunit;
using MauiApp1.Services;

namespace MauiApp1.Tests
{
    public class PasswordHasherTests
    {
        [Fact]
        public void HashAndVerify_CorrectPassword_ReturnsTrue()
        {
            string password = "P@ssw0rd!";
            var (hash, salt, iterations, alg) = PasswordHasher.HashPassword(password);

            Assert.False(string.IsNullOrEmpty(hash));
            Assert.False(string.IsNullOrEmpty(salt));
            Assert.Equal(PasswordHasher.AlgorithmName, alg);
            Assert.True(iterations > 0);

            bool ok = PasswordHasher.VerifyPassword(password, hash, salt, iterations);
            Assert.True(ok);
        }

        [Fact]
        public void Verify_WrongPassword_ReturnsFalse()
        {
            string password = "P@ssw0rd!";
            var (hash, salt, iterations, alg) = PasswordHasher.HashPassword(password);

            bool ok = PasswordHasher.VerifyPassword("wrong", hash, salt, iterations);
            Assert.False(ok);
        }
    }
}
