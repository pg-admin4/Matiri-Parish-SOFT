using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http.Headers;

namespace MauiApp1.Services;

public class AuthMessageHandler : DelegatingHandler
{
    private readonly IAuthService _auth;

    public AuthMessageHandler(IAuthService auth)
    {
        _auth = auth;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = await _auth.GetAccessTokenAsync();
        if (!string.IsNullOrEmpty(token))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await base.SendAsync(request, cancellationToken);

        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            response.Dispose();
            bool refreshed = await _auth.RefreshTokenAsync();
            if (refreshed)
            {
                var newToken = await _auth.GetAccessTokenAsync();
                if (!string.IsNullOrEmpty(newToken))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", newToken);
                    return await base.SendAsync(request, cancellationToken);
                }
            }
        }

        return response;
    }
}
