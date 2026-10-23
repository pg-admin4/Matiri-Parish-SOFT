using System;
using System.Threading.Tasks;
using MatiriParish.Core.Models;

namespace MatiriParish.Core.Services
{
    public interface IAuthService
    {
        UserRole? CurrentRole { get; }
        string CurrentUsername { get; }

        Task<bool> RegisterInitialAdminAsync(string username, string password);
        Task<LoginResult> LoginAsync(string username, string password);
        Task<bool> IsMfaEnabledForUserAsync(string username);
        string GenerateOtp(string username);
        bool ValidateOtp(string username, string inputOtp);
        Task<bool> ResetPasswordAsync(string username, string newPassword);
        void Logout();

        Task<bool> SignInAsync(string username, string password);
        Task LogoutAsync();
        Task<AuthTokens?> GetStoredTokensAsync();
        Task<string?> GetAccessTokenAsync();
        Task<bool> RefreshTokenAsync();
        // Platform-specific/OAuth sign-in helpers
        Task SignInWithGoogleAsync();
        // Persist externally-obtained OAuth tokens into the app's key-value store
        Task StoreExternalTokensAsync(Models.AuthTokens tokens);
    }
}
