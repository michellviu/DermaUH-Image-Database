using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.JSInterop;
using Web.DermaImage.Shared.Models;

namespace Web.DermaImage.Services;

public partial class AuthService
{
    public async Task<UserProfile?> FetchProfileAsync()
    {
        try
        {
            var profile = await _http.GetFromJsonAsync<UserProfile>("/api/auth/profile",
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (profile is not null)
            {
                _cachedUser = profile;
                await _js.InvokeVoidAsync("localStorage.setItem", UserKey, JsonSerializer.Serialize(profile));
            }

            return profile;
        }
        catch
        {
            return null;
        }
    }

    public async Task<(bool Success, string? Error)> UpdateProfileAsync(UpdateProfileRequest request)
    {
        try
        {
            var response = await _http.PutAsJsonAsync("/api/auth/profile", request);
            if (!response.IsSuccessStatusCode)
            {
                return (false, await TryGetError(response));
            }

            await FetchProfileAsync();
            AuthStateChanged?.Invoke();
            return (true, null);
        }
        catch (Exception ex)
        {
            return (false, $"Error de conexión: {ex.Message}");
        }
    }

    public async Task<(bool Success, string? Error)> ChangePasswordAsync(ChangePasswordRequest request)
    {
        try
        {
            var response = await _http.PutAsJsonAsync("/api/auth/change-password", new
            {
                request.CurrentPassword,
                NewPassword = request.NewPassword,
            });

            if (!response.IsSuccessStatusCode)
            {
                return (false, await TryGetError(response));
            }

            return (true, null);
        }
        catch (Exception ex)
        {
            return (false, $"Error de conexión: {ex.Message}");
        }
    }

}
