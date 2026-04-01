using Microsoft.Extensions.DependencyInjection;

namespace Web.DermaImage.Services;

/// <summary>
/// DelegatingHandler that attaches the JWT Bearer token to every outbound HTTP request.
/// Uses IServiceProvider to lazily resolve AuthService, breaking the circular dependency:
/// HttpClient → AuthenticatedHttpClientHandler → AuthService → HttpClient.
/// </summary>
public class AuthenticatedHttpClientHandler : DelegatingHandler
{
    private readonly IServiceProvider _serviceProvider;

    public AuthenticatedHttpClientHandler(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var authService = _serviceProvider.GetRequiredService<AuthService>();
        var token = await authService.GetTokenAsync();
        if (!string.IsNullOrWhiteSpace(token))
        {
            request.Headers.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
