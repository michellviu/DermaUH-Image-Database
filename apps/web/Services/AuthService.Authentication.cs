using System.Net.Http.Json;
using System.Text.Json;
using Web.DermaImage.Shared.Models;

namespace Web.DermaImage.Services;

public partial class AuthService
{
    public async Task<(bool Success, string? Error)> LoginAsync(LoginRequest request)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("/api/auth/login", request);
            if (!response.IsSuccessStatusCode)
            {
                var err = await TryGetError(response);
                return (false, err);
            }

            var loginResp = await response.Content.ReadFromJsonAsync<LoginResponse>(
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (loginResp is null)
            {
                return (false, "Respuesta inesperada del servidor.");
            }

            await PersistSession(loginResp);
            return (true, null);
        }
        catch (Exception ex)
        {
            return (false, $"Error de conexión: {ex.Message}");
        }
    }

    public async Task<(bool Success, string? Error)> RegisterAsync(RegisterRequest request)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("/api/auth/register", new
            {
                request.FirstName,
                request.LastName,
                request.Email,
                request.Password,
                request.InstitutionId,
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

    public async Task<(bool Success, string? Error)> GoogleLoginAsync(string idToken)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("/api/auth/google",
                new GoogleLoginRequest { IdToken = idToken });

            if (!response.IsSuccessStatusCode)
            {
                return (false, await TryGetError(response));
            }

            var loginResp = await response.Content.ReadFromJsonAsync<LoginResponse>(
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (loginResp is null)
            {
                return (false, "Respuesta inesperada del servidor.");
            }

            await PersistSession(loginResp);
            return (true, null);
        }
        catch (Exception ex)
        {
            return (false, $"Error de conexión: {ex.Message}");
        }
    }

    public async Task<(bool Success, string? Error)> ConfirmEmailAsync(string userId, string token)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("/api/auth/confirm-email",
                new ConfirmEmailRequest { UserId = userId, Token = token });

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

    public async Task<(bool Success, string? Error)> ForgotPasswordAsync(string email)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("/api/auth/forgot-password",
                new ForgotPasswordRequest { Email = email });

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

    public async Task<(bool Success, string? Error)> ResetPasswordAsync(ResetPasswordRequest request)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("/api/auth/reset-password", new
            {
                request.UserId,
                request.Token,
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
