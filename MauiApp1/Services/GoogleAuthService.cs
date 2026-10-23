using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Maui.Authentication;
using MatiriParish.Core.Models;
using MatiriParish.Core.Services;

namespace MauiApp1.Services
{
    public class GoogleAuthService
    {
        // TODO: replace with your Google OAuth client id and redirect
        private readonly string _clientId = "<GOOGLE_CLIENT_ID>";
        private readonly string _redirectUri = "com.yourcompany.app:/oauth2redirect";
        private readonly HttpClient _http = new HttpClient();

        public async Task StartInteractiveSignInAsync()
        {
            var (codeVerifier, codeChallenge) = CreatePkce();

            var authUrl = new Uri($"https://accounts.google.com/o/oauth2/v2/auth" +
                                  $"?client_id={Uri.EscapeDataString(_clientId)}" +
                                  $"&redirect_uri={Uri.EscapeDataString(_redirectUri)}" +
                                  $"&response_type=code" +
                                  $"&scope={Uri.EscapeDataString("openid profile email")}" +
                                  $"&code_challenge={codeChallenge}" +
                                  $"&code_challenge_method=S256" +
                                  $"&access_type=offline" +
                                  $"&prompt=consent");

            var callbackUrl = new Uri(_redirectUri);

            var result = await WebAuthenticator.Default.AuthenticateAsync(authUrl, callbackUrl);
            if (!result.Properties.TryGetValue("code", out var code))
                throw new InvalidOperationException("OAuth code not returned.");

            var tokens = await ExchangeCodeForTokensAsync(code, codeVerifier);

            // Persist tokens via core AuthService (if available)
            var auth = AuthService.Shared;
            if (auth != null)
            {
                await auth.StoreExternalTokensAsync(new MatiriParish.Core.Models.AuthTokens
                {
                    AccessToken = tokens.AccessToken ?? string.Empty,
                    RefreshToken = tokens.RefreshToken ?? string.Empty,
                    ExpiresAtUtc = DateTime.UtcNow.AddSeconds(tokens.ExpiresIn)
                });
            }
        }

        private async Task<(string AccessToken, string? RefreshToken, int ExpiresIn)> ExchangeCodeForTokensAsync(string code, string codeVerifier)
        {
            var tokenEndpoint = "https://oauth2.googleapis.com/token";
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string,string>("code", code),
                new KeyValuePair<string,string>("client_id", _clientId),
                new KeyValuePair<string,string>("redirect_uri", _redirectUri),
                new KeyValuePair<string,string>("grant_type", "authorization_code"),
                new KeyValuePair<string,string>("code_verifier", codeVerifier)
            });

            var res = await _http.PostAsync(tokenEndpoint, content);
            res.EnsureSuccessStatusCode();
            var json = await res.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            var access = root.GetProperty("access_token").GetString();
            var refresh = root.TryGetProperty("refresh_token", out var r) ? r.GetString() : null;
            var expires = root.TryGetProperty("expires_in", out var e) ? e.GetInt32() : 3600;
            return (access ?? string.Empty, refresh, expires);
        }

        private static (string codeVerifier, string codeChallenge) CreatePkce()
        {
            var bytes = new byte[64];
            System.Security.Cryptography.RandomNumberGenerator.Fill(bytes);
            var codeVerifier = Base64UrlEncode(bytes);
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var challengeBytes = sha256.ComputeHash(System.Text.Encoding.ASCII.GetBytes(codeVerifier));
            var codeChallenge = Base64UrlEncode(challengeBytes);
            return (codeVerifier, codeChallenge);

            static string Base64UrlEncode(byte[] arg)
            {
                return Convert.ToBase64String(arg)
                    .TrimEnd('=')
                    .Replace('+', '-')
                    .Replace('/', '_');
            }
        }
    }
}
