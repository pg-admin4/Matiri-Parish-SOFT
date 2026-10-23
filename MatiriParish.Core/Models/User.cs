using System;

namespace MatiriParish.Core.Models
{
    public enum UserRole { User = 0, Admin = 1 }

    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string PasswordSalt { get; set; } = string.Empty;
        public int Iterations { get; set; }
        public string PasswordHashAlgorithm { get; set; } = string.Empty;
        public bool IsMfaEnabled { get; set; }
        public UserRole Role { get; set; }

        // Lockout fields simplified
        public int FailedLoginCount { get; set; }
        public DateTime? LockoutEndUtc { get; set; }
        public DateTime? LastFailedLoginUtc { get; set; }
    }
}
