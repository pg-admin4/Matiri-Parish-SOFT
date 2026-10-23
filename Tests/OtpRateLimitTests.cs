using System;
using Xunit;
using MauiApp1.Services;

namespace MauiApp1.Tests;

public class OtpRateLimitTests
{
    [Fact]
    public void GenerateOtp_RateLimitExceeded_ReturnsEmptyString()
    {
        var auth = new AuthService();
        string username = "testuser_" + Guid.NewGuid().ToString("N");

        // Generate MaxOtpsPerWindow times should return codes
        for (int i = 0; i < 3; i++)
        {
            string code = auth.GenerateOtp(username);
            Assert.False(code == null);
            // allow either code or empty if timing issues, but expect first three to be non-empty
            Assert.False(string.IsNullOrEmpty(code));
        }

        // Fourth generation should be rate-limited
        string fourth = auth.GenerateOtp(username);
        Assert.True(string.IsNullOrEmpty(fourth));
    }

    [Fact]
    public void ValidateOtp_FailedAttempts_BlockFurtherVerification()
    {
        var auth = new AuthService();
        string username = "testuser_" + Guid.NewGuid().ToString("N");

        string code = auth.GenerateOtp(username);
        Assert.False(string.IsNullOrEmpty(code));

        // Make MaxVerifyAttempts wrong attempts
        for (int i = 0; i < 5; i++)
        {
            bool valid = auth.ValidateOtp(username, "000000");
            Assert.False(valid);
        }

        // Now even the correct code should fail because verification is blocked
        bool now = auth.ValidateOtp(username, code);
        Assert.False(now);
    }
}
