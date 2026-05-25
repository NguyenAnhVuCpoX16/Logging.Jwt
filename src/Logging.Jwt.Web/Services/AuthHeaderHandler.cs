namespace Logging.Jwt.Web.Services;

public class AuthHeaderHandler(TokenStorage tokenStorage) : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(tokenStorage.AccessToken))
        {
            request.Headers.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenStorage.AccessToken);
        }

        return base.SendAsync(request, cancellationToken);
    }
}
