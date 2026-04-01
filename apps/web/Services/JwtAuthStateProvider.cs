using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;

namespace Web.DermaImage.Services;

/// <summary>
/// Custom Blazor AuthenticationStateProvider backed by the JWT stored in localStorage.
/// </summary>
public class JwtAuthStateProvider : AuthenticationStateProvider
{
    private readonly AuthService _authService;

    private static readonly AuthenticationState Anonymous =
        new(new ClaimsPrincipal(new ClaimsIdentity()));

    public JwtAuthStateProvider(AuthService authService)
    {
        _authService = authService;
        _authService.AuthStateChanged += OnAuthStateChanged;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await _authService.GetTokenAsync();
        if (string.IsNullOrWhiteSpace(token))
            return Anonymous;

        var claims = ParseClaimsFromJwt(token);
        if (claims is null)
            return Anonymous;

        // Check expiry
        var expClaim = claims.FirstOrDefault(c => c.Type == "exp");
        if (expClaim is not null && long.TryParse(expClaim.Value, out var exp))
        {
            var expiry = DateTimeOffset.FromUnixTimeSeconds(exp);
            if (expiry < DateTimeOffset.UtcNow)
            {
                await _authService.LogoutAsync();
                return Anonymous;
            }
        }

        var identity = new ClaimsIdentity(claims, "jwt");
        return new AuthenticationState(new ClaimsPrincipal(identity));
    }

    private void OnAuthStateChanged()
    {
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    private static List<Claim>? ParseClaimsFromJwt(string jwt)
    {
        try
        {
            var parts = jwt.Split('.');
            if (parts.Length != 3) return null;

            var payload = parts[1];
            // Pad base64url string to a multiple of 4
            var remainder = payload.Length % 4;
            if (remainder == 2) payload += "==";
            else if (remainder == 3) payload += "=";
            payload = payload.Replace('-', '+').Replace('_', '/');

            var bytes = Convert.FromBase64String(payload);
            var json  = System.Text.Encoding.UTF8.GetString(bytes);

            var dict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);
            if (dict is null) return null;

            var claims = new List<Claim>();

            foreach (var kv in dict)
            {
                if (kv.Value.ValueKind == JsonValueKind.Array)
                {
                    foreach (var item in kv.Value.EnumerateArray())
                        claims.Add(new Claim(kv.Key, item.GetString() ?? string.Empty));
                }
                else
                {
                    claims.Add(new Claim(kv.Key, kv.Value.ToString()));
                }
            }

            // Map standard JWT claim names to .NET claim types
            MapClaim(claims, "sub",    ClaimTypes.NameIdentifier);
            MapClaim(claims, "email",  ClaimTypes.Email);
            MapClaim(claims, "role",   ClaimTypes.Role);
            MapClaim(claims, "http://schemas.microsoft.com/ws/2008/06/identity/claims/role", ClaimTypes.Role);

            return claims;
        }
        catch
        {
            return null;
        }
    }

    private static void MapClaim(List<Claim> claims, string fromType, string toType)
    {
        var existing = claims.Where(c => c.Type == fromType).ToList();
        foreach (var c in existing)
        {
            if (!claims.Any(x => x.Type == toType && x.Value == c.Value))
                claims.Add(new Claim(toType, c.Value));
        }
    }
}
