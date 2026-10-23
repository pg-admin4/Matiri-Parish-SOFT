using System;

namespace MauiApp1.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string PasswordSalt { get; set; } = string.Empty;
        public UserRole Role { get; set; } = UserRole.StandardUser;

        // MFA Fields
        public bool IsMfaEnabled { get; set; } = false;
        public string? MfaSecretKey { get; set; } // Base32 TOTP secret key

        // Password hashing metadata (versioning and iterations)
        // Iterations allows per-user tuning and future migration
        public int Iterations { get; set; } = 0;
        // HashAlgorithm indicates the algorithm/version used to compute the PasswordHash
        public string PasswordHashAlgorithm { get; set; } = string.Empty;

        // Account lockout and failed login tracking
        // Number of consecutive failed login attempts
        public int FailedLoginCount { get; set; } = 0;
        // When the account lockout expires (UTC). Null when not locked out.
        public DateTime? LockoutEndUtc { get; set; }
        // Timestamp of the last failed login attempt (UTC)
        public DateTime? LastFailedLoginUtc { get; set; }
    }
}