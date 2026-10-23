namespace MatiriParish.Core.Models
{
    public enum LoginStatus { Success = 0, Invalid = 1, LockedOut = 2, Error = 3 }

    public sealed class LoginResult
    {
        public LoginStatus Status { get; init; }
        public string? Message { get; init; }
        public System.DateTime? LockedUntilUtc { get; init; }

        private LoginResult() { }

        public static LoginResult Success() => new LoginResult { Status = LoginStatus.Success };
        public static LoginResult Invalid() => new LoginResult { Status = LoginStatus.Invalid };
        public static LoginResult LockedOut(System.DateTime untilUtc, string? message = null) => new LoginResult { Status = LoginStatus.LockedOut, LockedUntilUtc = untilUtc, Message = message };
        public static LoginResult Error(string? message) => new LoginResult { Status = LoginStatus.Error, Message = message };
    }
}
