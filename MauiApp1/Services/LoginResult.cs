using System;

namespace MauiApp1.Services;

public enum LoginStatus
{
    Success,
    InvalidCredentials,
    LockedOut,
    Error
}

public sealed class LoginResult
{
    public LoginStatus Status { get; init; }
    public string Message { get; init; } = string.Empty;
    public DateTime? LockoutEndUtc { get; init; }

    public static LoginResult Success() => new() { Status = LoginStatus.Success };
    public static LoginResult Invalid(string message = "Invalid username or password.") => new() { Status = LoginStatus.InvalidCredentials, Message = message };
    public static LoginResult LockedOut(DateTime until, string message = "Account is locked.") => new() { Status = LoginStatus.LockedOut, LockoutEndUtc = until, Message = message };
    public static LoginResult Error(string message) => new() { Status = LoginStatus.Error, Message = message };
}
