namespace MatiriParish.Core.Models
{
    public sealed class AuthTokens
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public System.DateTime ExpiresAtUtc { get; set; }
    }
}
