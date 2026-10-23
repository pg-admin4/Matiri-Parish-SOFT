using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MatiriParish.Core.Data;
using MatiriParish.Core.Models;

namespace MatiriParish.Core.Services
{
    public class AuthService : IAuthService
    {
        private const int Pbkdf2Iterations = 120_000; // tuned for client devices (adjust as needed)
        private const int HashByteSize = 32;
        private const int SaltByteSize = 16;

        private sealed record OtpInfo(string Code, DateTime Expiry);
        private static readonly ConcurrentDictionary<string, OtpInfo> UserOtps = new(StringComparer.OrdinalIgnoreCase);

        private sealed class OtpRateInfo
        {
            public readonly object Lock = new();
            public readonly System.Collections.Generic.List<DateTime> GenerationTimes = new();
            public int FailedVerifications = 0;
            public DateTime? VerifyBlockEndUtc = null;
            public DateTime? LastFailedLoginUtc = null;
        }

        /// <summary>
        /// Stub for Google OAuth sign-in. This does not perform a real OAuth flow.
        /// Implement platform-specific OAuth in app layers and call into core to persist tokens.
        /// </summary>
        public async Task SignInWithGoogleAsync()
        {
            // Placeholder implementation: log and return.
            Debug.WriteLine("[AUTH] SignInWithGoogleAsync called (stub). Implement OAuth in platform project.");
            await Task.CompletedTask;
        }

        /// <summary>
        /// Persist OAuth tokens obtained by platform code into the configured IKeyValueStore.
        /// </summary>
        public async Task StoreExternalTokensAsync(AuthTokens tokens)
        {
            if (tokens == null) throw new ArgumentNullException(nameof(tokens));
            await StoreTokensAsync(tokens);
        }

        private static readonly ConcurrentDictionary<string, OtpRateInfo> OtpRates = new(StringComparer.OrdinalIgnoreCase);
        private const int MaxOtpsPerWindow = 3;
        private const int OtpWindowMinutes = 15;
        private const int MaxVerifyAttempts = 5;
        private const int VerifyBlockMinutes = 15;

        private const string Key_Access = "auth_access";
        private const string Key_Refresh = "auth_refresh";
        private const string Key_Expires = "auth_expires";

        public UserRole? CurrentRole { get; private set; }
        public string CurrentUsername { get; private set; } = string.Empty;

        // For tests and DI we accept an IDbContextFactory and a key-value store abstraction
        private readonly IDbContextFactory<MatiriParishDbContext> _dbFactory;
        private readonly IKeyValueStore _kvStore;

        public AuthService(IDbContextFactory<MatiriParishDbContext> dbFactory, IKeyValueStore kvStore)
        {
            _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
            _kvStore = kvStore ?? throw new ArgumentNullException(nameof(kvStore));
        }

        // Static shared facade for backward compatibility (can be set by host)
        public static IAuthService? Shared { get; set; }

        public async Task<bool> RegisterInitialAdminAsync(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password)) return false;

            try
            {
                await using var db = _dbFactory.CreateDbContext();
                bool hasAdmin = await db.Users.AnyAsync(u => u.Role == UserRole.Admin);
                if (hasAdmin) return false;

                byte[] saltBytes = RandomNumberGenerator.GetBytes(SaltByteSize);
                string salt = Convert.ToBase64String(saltBytes);
                using var pbkdf2 = new Rfc2898DeriveBytes(password, saltBytes, Pbkdf2Iterations, HashAlgorithmName.SHA256);
                string hash = Convert.ToBase64String(pbkdf2.GetBytes(HashByteSize));

                var user = new User
                {
                    Username = username.Trim(),
                    PasswordSalt = salt,
                    PasswordHash = hash,
                    Role = UserRole.Admin,
                    Iterations = Pbkdf2Iterations,
                    PasswordHashAlgorithm = "PBKDF2-SHA256-v1"
                };

                db.Users.Add(user);
                await db.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[AUTH ERROR] RegisterInitialAdmin failed: {ex.Message}");
                return false;
            }
        }

        public async Task<LoginResult> LoginAsync(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                return LoginResult.Invalid();

            try
            {
                await using var db = _dbFactory.CreateDbContext();
                string normalizedUsername = username.Trim().ToLowerInvariant();

                var user = await db.Users.FirstOrDefaultAsync(u => u.Username.ToLower() == normalizedUsername);
                if (user is null) return LoginResult.Invalid();

                if (user.LockoutEndUtc.HasValue && DateTime.UtcNow < user.LockoutEndUtc.Value)
                {
                    return LoginResult.LockedOut(user.LockoutEndUtc.Value, $"Account locked until {user.LockoutEndUtc.Value:u}");
                }

                int iterationsToUse = user.Iterations > 0 ? user.Iterations : Pbkdf2Iterations;
                bool verified = PasswordHasher.VerifyPassword(password, user.PasswordHash, user.PasswordSalt, iterationsToUse);

                if (verified)
                {
                    user.FailedLoginCount = 0;
                    user.LockoutEndUtc = null;
                    user.LastFailedLoginUtc = null;
                    await db.SaveChangesAsync();

                    CurrentRole = user.Role;
                    CurrentUsername = user.Username;

                    try
                    {
                        await _kvStore.SetAsync("auth_username", CurrentUsername);
                        await _kvStore.SetAsync("auth_role", ((int)CurrentRole).ToString());
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"[AUTH] KeyValue store unavailable: {ex.Message}");
                    }

                    return LoginResult.Success();
                }

                user.FailedLoginCount = (user.FailedLoginCount >= 0) ? user.FailedLoginCount + 1 : 1;
                user.LastFailedLoginUtc = DateTime.UtcNow;

                if (user.FailedLoginCount >= 5)
                {
                    int exponent = Math.Max(0, user.FailedLoginCount - 5);
                    double minutes = 5 * Math.Pow(2, exponent);
                    int lockoutMinutes = (int)Math.Min(60 * 24, Math.Max(1, Math.Round(minutes)));
                    user.LockoutEndUtc = DateTime.UtcNow.AddMinutes(lockoutMinutes);
                    await db.SaveChangesAsync();
                    return LoginResult.LockedOut(user.LockoutEndUtc.Value, $"Account locked until {user.LockoutEndUtc.Value:u}");
                }

                await db.SaveChangesAsync();
                return LoginResult.Invalid();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[AUTH ERROR] Login failed: {ex.Message}");
                return LoginResult.Error(ex.Message);
            }
        }

        public async Task<bool> IsMfaEnabledForUserAsync(string username)
        {
            if (string.IsNullOrWhiteSpace(username)) return false;
            try
            {
                await using var db = _dbFactory.CreateDbContext();
                string normalizedUsername = username.Trim().ToLowerInvariant();
                var user = await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Username.ToLower() == normalizedUsername);
                return user?.IsMfaEnabled ?? false;
            }
            catch
            {
                return false;
            }
        }

        public string GenerateOtp(string username)
        {
            if (string.IsNullOrWhiteSpace(username)) throw new ArgumentException("Username required for OTP generation.", nameof(username));
            string normalizedUsername = username.Trim().ToLowerInvariant();
            var rateInfo = OtpRates.GetOrAdd(normalizedUsername, _ => new OtpRateInfo());
            lock (rateInfo.Lock)
            {
                DateTime windowStart = DateTime.UtcNow.AddMinutes(-OtpWindowMinutes);
                rateInfo.GenerationTimes.RemoveAll(t => t < windowStart);
                if (rateInfo.GenerationTimes.Count >= MaxOtpsPerWindow)
                {
                    return string.Empty;
                }
                string otpCode = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
                var otpInfo = new OtpInfo(otpCode, DateTime.UtcNow.AddMinutes(3));
                UserOtps[normalizedUsername] = otpInfo;
                rateInfo.GenerationTimes.Add(DateTime.UtcNow);
                rateInfo.FailedVerifications = 0;
                rateInfo.VerifyBlockEndUtc = null;
                return otpCode;
            }
        }

        public bool ValidateOtp(string username, string inputOtp)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(inputOtp)) return false;
            string normalizedUsername = username.Trim().ToLowerInvariant();
            var rateInfo = OtpRates.GetOrAdd(normalizedUsername, _ => new OtpRateInfo());
            lock (rateInfo.Lock)
            {
                if (rateInfo.VerifyBlockEndUtc.HasValue && DateTime.UtcNow < rateInfo.VerifyBlockEndUtc.Value)
                {
                    return false;
                }
                if (!UserOtps.TryGetValue(normalizedUsername, out var otpInfo)) return false;
                if (DateTime.UtcNow > otpInfo.Expiry)
                {
                    UserOtps.TryRemove(normalizedUsername, out _);
                    return false;
                }
                byte[] inputBytes = Encoding.UTF8.GetBytes(inputOtp.Trim());
                byte[] storedBytes = Encoding.UTF8.GetBytes(otpInfo.Code);
                bool isValid = CryptographicOperations.FixedTimeEquals(inputBytes, storedBytes);
                if (isValid)
                {
                    UserOtps.TryRemove(normalizedUsername, out _);
                    rateInfo.FailedVerifications = 0;
                    rateInfo.VerifyBlockEndUtc = null;
                    return true;
                }
                rateInfo.FailedVerifications++;
                if (rateInfo.FailedVerifications >= MaxVerifyAttempts)
                {
                    rateInfo.VerifyBlockEndUtc = DateTime.UtcNow.AddMinutes(VerifyBlockMinutes);
                    UserOtps.TryRemove(normalizedUsername, out _);
                }
                return false;
            }
        }

        public async Task<bool> ResetPasswordAsync(string username, string newPassword)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(newPassword)) return false;
            try
            {
                await using var db = _dbFactory.CreateDbContext();
                string normalizedUsername = username.Trim().ToLowerInvariant();
                var user = await db.Users.FirstOrDefaultAsync(u => u.Username.ToLower() == normalizedUsername);
                if (user is null) return false;
                var (hash, saltBase64, iters, alg) = MatiriParish.Core.Services.PasswordHasher.HashPassword(newPassword, Pbkdf2Iterations);
                user.PasswordSalt = saltBase64;
                user.PasswordHash = hash;
                user.Iterations = iters;
                user.PasswordHashAlgorithm = alg;
                db.Users.Update(user);
                await db.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[AUTH ERROR] ResetPassword failed: {ex.Message}");
                return false;
            }
        }

        public void Logout()
        {
            CurrentRole = null;
            CurrentUsername = string.Empty;
            try
            {
                _kvStore.SetAsync("auth_username", string.Empty).Wait();
                _kvStore.SetAsync("auth_role", string.Empty).Wait();
            }
            catch { }
        }

        private async Task StoreTokensAsync(AuthTokens tokens)
        {
            try
            {
                await _kvStore.SetAsync(Key_Access, tokens.AccessToken);
                await _kvStore.SetAsync(Key_Refresh, tokens.RefreshToken);
                await _kvStore.SetAsync(Key_Expires, tokens.ExpiresAtUtc.ToString("o"));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[AUTH] Failed to store tokens: {ex.Message}");
            }
        }

        private async Task<AuthTokens?> ReadTokensAsync()
        {
            try
            {
                var a = await _kvStore.GetAsync(Key_Access);
                var r = await _kvStore.GetAsync(Key_Refresh);
                var e = await _kvStore.GetAsync(Key_Expires);
                if (string.IsNullOrEmpty(a) || string.IsNullOrEmpty(r) || string.IsNullOrEmpty(e)) return null;
                if (!DateTime.TryParse(e, null, System.Globalization.DateTimeStyles.RoundtripKind, out var exp)) return null;
                return new AuthTokens { AccessToken = a, RefreshToken = r, ExpiresAtUtc = exp };
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[AUTH] Failed to read tokens: {ex.Message}");
                return null;
            }
        }

        private async Task ClearTokensAsync()
        {
            try
            {
                await _kvStore.RemoveAsync(Key_Access);
                await _kvStore.RemoveAsync(Key_Refresh);
                await _kvStore.RemoveAsync(Key_Expires);
            }
            catch { }
        }

        public async Task<bool> SignInAsync(string username, string password)
        {
            var login = await LoginAsync(username, password);
            if (login.Status != LoginStatus.Success) return false;
            var tokens = new AuthTokens
            {
                AccessToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)),
                RefreshToken = Guid.NewGuid().ToString("N"),
                ExpiresAtUtc = DateTime.UtcNow.AddMinutes(15)
            };
            await StoreTokensAsync(tokens);
            return true;
        }

        public async Task LogoutAsync()
        {
            Logout();
            await ClearTokensAsync();
        }

        public Task<AuthTokens?> GetStoredTokensAsync() => ReadTokensAsync();

        public async Task<string?> GetAccessTokenAsync()
        {
            var tokens = await ReadTokensAsync();
            if (tokens is null) return null;
            if (tokens.ExpiresAtUtc <= DateTime.UtcNow.AddSeconds(60))
            {
                bool ok = await RefreshTokenAsync();
                if (!ok) return null;
                tokens = await ReadTokensAsync();
                return tokens?.AccessToken;
            }
            return tokens.AccessToken;
        }

        public async Task<bool> RefreshTokenAsync()
        {
            var tokens = await ReadTokensAsync();
            if (tokens is null || string.IsNullOrEmpty(tokens.RefreshToken)) return false;
            var newTokens = new AuthTokens
            {
                AccessToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)),
                RefreshToken = Guid.NewGuid().ToString("N"),
                ExpiresAtUtc = DateTime.UtcNow.AddMinutes(15)
            };
            await StoreTokensAsync(newTokens);
            return true;
        }
    }
}
