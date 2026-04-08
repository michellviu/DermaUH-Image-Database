using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.JSInterop;
using Web.DermaImage.Shared.Models;

namespace Web.DermaImage.Services;

/// <summary>
/// Manages authentication state, token storage, and API auth calls.
/// </summary>
public partial class AuthService
{
    private const string TokenKey = "dermauh_jwt";
    private const string UserKey = "dermauh_user";

    private readonly HttpClient _http;
    private readonly IJSRuntime _js;

    private string? _cachedToken;
    private UserProfile? _cachedUser;

    public event Action? AuthStateChanged;

    public AuthService(HttpClient http, IJSRuntime js)
    {
        _http = http;
        _js = js;
    }

    public async Task<string?> GetTokenAsync()
    {
        if (_cachedToken is not null)
        {
            return _cachedToken;
        }

        try
        {
            _cachedToken = await _js.InvokeAsync<string?>("localStorage.getItem", TokenKey);
        }
        catch
        {
            // JS not ready yet (prerender).
        }

        return _cachedToken;
    }

    public async Task<UserProfile?> GetCurrentUserAsync()
    {
        if (_cachedUser is not null)
        {
            return _cachedUser;
        }

        try
        {
            var json = await _js.InvokeAsync<string?>("localStorage.getItem", UserKey);
            if (!string.IsNullOrEmpty(json))
            {
                _cachedUser = JsonSerializer.Deserialize<UserProfile>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
        }
        catch
        {
            // Ignore deserialization/storage errors.
        }

        return _cachedUser;
    }

    public bool IsAuthenticated => _cachedToken is not null;

    public async Task LogoutAsync()
    {
        _cachedToken = null;
        _cachedUser = null;

        try
        {
            await _js.InvokeVoidAsync("localStorage.removeItem", TokenKey);
            await _js.InvokeVoidAsync("localStorage.removeItem", UserKey);
        }
        catch
        {
            // Ignore localStorage cleanup failures.
        }

        AuthStateChanged?.Invoke();
    }

    private async Task PersistSession(LoginResponse loginResp)
    {
        _cachedToken = loginResp.Token;
        _cachedUser = new UserProfile
        {
            Id = loginResp.UserId,
            Email = loginResp.Email,
            FirstName = loginResp.FullName.Split(' ').FirstOrDefault() ?? string.Empty,
            LastName = loginResp.FullName.Contains(' ')
                ? loginResp.FullName[(loginResp.FullName.IndexOf(' ') + 1)..]
                : string.Empty,
            Roles = loginResp.Roles,
            EmailConfirmed = loginResp.EmailConfirmed,
        };

        try
        {
            await _js.InvokeVoidAsync("localStorage.setItem", TokenKey, _cachedToken);
            await _js.InvokeVoidAsync("localStorage.setItem", UserKey, JsonSerializer.Serialize(_cachedUser));
        }
        catch
        {
            // Ignore localStorage persistence failures.
        }

        AuthStateChanged?.Invoke();
    }
}
