using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.JSInterop;
using Web.DermaImage.Shared.Models;

namespace Web.DermaImage.Services;

/// <summary>
/// Manages authentication state, token storage, and API auth calls.
/// </summary>
public class AuthService
{
    private const string TokenKey = "dermauh_jwt";
    private const string UserKey  = "dermauh_user";

    private readonly HttpClient _http;
    private readonly IJSRuntime _js;

    private string? _cachedToken;
    private UserProfile? _cachedUser;

    public event Action? AuthStateChanged;

    public AuthService(HttpClient http, IJSRuntime js)
    {
        _http = http;
        _js   = js;
    }

    // ── Token persistence ───────────────────────────────────────────────

    public async Task<string?> GetTokenAsync()
    {
        if (_cachedToken is not null)
            return _cachedToken;

        try
        {
            _cachedToken = await _js.InvokeAsync<string?>("localStorage.getItem", TokenKey);
        }
        catch { /* JS not ready yet (prerender) */ }

        return _cachedToken;
    }

    public async Task<UserProfile?> GetCurrentUserAsync()
    {
        if (_cachedUser is not null)
            return _cachedUser;

        try
        {
            var json = await _js.InvokeAsync<string?>("localStorage.getItem", UserKey);
            if (!string.IsNullOrEmpty(json))
                _cachedUser = JsonSerializer.Deserialize<UserProfile>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch { }

        return _cachedUser;
    }

    public bool IsAuthenticated => _cachedToken is not null;

    // ── Login ───────────────────────────────────────────────────────────

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
                return (false, "Respuesta inesperada del servidor.");

            await PersistSession(loginResp);
            return (true, null);
        }
        catch (Exception ex)
        {
            return (false, $"Error de conexión: {ex.Message}");
        }
    }

    // ── Register ────────────────────────────────────────────────────────

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
                return (false, await TryGetError(response));

            return (true, null);
        }
        catch (Exception ex)
        {
            return (false, $"Error de conexión: {ex.Message}");
        }
    }

    // ── Google Login ────────────────────────────────────────────────────

    public async Task<(bool Success, string? Error)> GoogleLoginAsync(string idToken)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("/api/auth/google",
                new GoogleLoginRequest { IdToken = idToken });

            if (!response.IsSuccessStatusCode)
                return (false, await TryGetError(response));

            var loginResp = await response.Content.ReadFromJsonAsync<LoginResponse>(
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (loginResp is null)
                return (false, "Respuesta inesperada del servidor.");

            await PersistSession(loginResp);
            return (true, null);
        }
        catch (Exception ex)
        {
            return (false, $"Error de conexión: {ex.Message}");
        }
    }

    // ── Email Confirmation ──────────────────────────────────────────────

    public async Task<(bool Success, string? Error)> ConfirmEmailAsync(string userId, string token)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("/api/auth/confirm-email",
                new ConfirmEmailRequest { UserId = userId, Token = token });

            if (!response.IsSuccessStatusCode)
                return (false, await TryGetError(response));

            return (true, null);
        }
        catch (Exception ex)
        {
            return (false, $"Error de conexión: {ex.Message}");
        }
    }

    // ── Forgot Password ─────────────────────────────────────────────────

    public async Task<(bool Success, string? Error)> ForgotPasswordAsync(string email)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("/api/auth/forgot-password",
                new ForgotPasswordRequest { Email = email });

            if (!response.IsSuccessStatusCode)
                return (false, await TryGetError(response));

            return (true, null);
        }
        catch (Exception ex)
        {
            return (false, $"Error de conexión: {ex.Message}");
        }
    }

    // ── Reset Password ──────────────────────────────────────────────────

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
                return (false, await TryGetError(response));

            return (true, null);
        }
        catch (Exception ex)
        {
            return (false, $"Error de conexión: {ex.Message}");
        }
    }

    // ── Profile ─────────────────────────────────────────────────────────

    public async Task<UserProfile?> FetchProfileAsync()
    {
        try
        {
            var profile = await _http.GetFromJsonAsync<UserProfile>("/api/auth/profile",
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (profile is not null)
            {
                _cachedUser = profile;
                await _js.InvokeVoidAsync("localStorage.setItem",
                    UserKey, JsonSerializer.Serialize(profile));
            }

            return profile;
        }
        catch { return null; }
    }

    public async Task<(bool Success, string? Error)> UpdateProfileAsync(UpdateProfileRequest request)
    {
        try
        {
            var response = await _http.PutAsJsonAsync("/api/auth/profile", request);
            if (!response.IsSuccessStatusCode)
                return (false, await TryGetError(response));

            // Refresh cached profile
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
                return (false, await TryGetError(response));

            return (true, null);
        }
        catch (Exception ex)
        {
            return (false, $"Error de conexión: {ex.Message}");
        }
    }

    public async Task<(bool Success, string? Error)> CreateInstitutionJoinRequestAsync(Guid institutionId)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("/api/institutions/join-requests", new CreateInstitutionJoinRequest
            {
                InstitutionId = institutionId,
            });

            if (!response.IsSuccessStatusCode)
                return (false, await TryGetError(response));

            return (true, null);
        }
        catch (Exception ex)
        {
            return (false, $"Error de conexión: {ex.Message}");
        }
    }

    public async Task<PagedResponse<InstitutionJoinRequestDto>> GetMyInstitutionJoinRequestsAsync(int page = 1, int pageSize = 5)
    {
        try
        {
            return await _http.GetFromJsonAsync<PagedResponse<InstitutionJoinRequestDto>>($"/api/institutions/join-requests/mine?page={page}&pageSize={pageSize}",
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                ?? new PagedResponse<InstitutionJoinRequestDto>
                {
                    Items = [],
                    Page = page,
                    PageSize = pageSize,
                };
        }
        catch
        {
            return new PagedResponse<InstitutionJoinRequestDto>
            {
                Items = [],
                Page = page,
                PageSize = pageSize,
            };
        }
    }

    public async Task<(bool Success, string? Error)> LeaveMyInstitutionAsync()
    {
        try
        {
            var response = await _http.PostAsync("/api/institutions/leave", content: null);
            if (!response.IsSuccessStatusCode)
                return (false, await TryGetError(response));

            await FetchProfileAsync();
            AuthStateChanged?.Invoke();
            return (true, null);
        }
        catch (Exception ex)
        {
            return (false, $"Error de conexión: {ex.Message}");
        }
    }

    public async Task<PagedResponse<InstitutionJoinRequestDto>> GetResponsibleInboxAsync(int page = 1, int pageSize = 5)
    {
        try
        {
            return await _http.GetFromJsonAsync<PagedResponse<InstitutionJoinRequestDto>>($"/api/institutions/join-requests/inbox?page={page}&pageSize={pageSize}",
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                ?? new PagedResponse<InstitutionJoinRequestDto>
                {
                    Items = [],
                    Page = page,
                    PageSize = pageSize,
                };
        }
        catch
        {
            return new PagedResponse<InstitutionJoinRequestDto>
            {
                Items = [],
                Page = page,
                PageSize = pageSize,
            };
        }
    }

    public async Task<(bool Success, string? Error)> ReviewInstitutionJoinRequestAsync(Guid requestId, ReviewInstitutionJoinRequest request)
    {
        try
        {
            var response = await _http.PostAsJsonAsync($"/api/institutions/join-requests/{requestId}/review", request);
            if (!response.IsSuccessStatusCode)
                return (false, await TryGetError(response));

            return (true, null);
        }
        catch (Exception ex)
        {
            return (false, $"Error de conexión: {ex.Message}");
        }
    }

    // ── Logout ──────────────────────────────────────────────────────────

    public async Task LogoutAsync()
    {
        _cachedToken = null;
        _cachedUser  = null;

        try
        {
            await _js.InvokeVoidAsync("localStorage.removeItem", TokenKey);
            await _js.InvokeVoidAsync("localStorage.removeItem", UserKey);
        }
        catch { }

        AuthStateChanged?.Invoke();
    }

    // ── Helpers ─────────────────────────────────────────────────────────

    private async Task PersistSession(LoginResponse loginResp)
    {
        _cachedToken = loginResp.Token;
        _cachedUser  = new UserProfile
        {
            Id             = loginResp.UserId,
            Email          = loginResp.Email,
            FirstName      = loginResp.FullName.Split(' ').FirstOrDefault() ?? string.Empty,
            LastName       = loginResp.FullName.Contains(' ')
                                 ? loginResp.FullName[(loginResp.FullName.IndexOf(' ') + 1)..]
                                 : string.Empty,
            Roles          = loginResp.Roles,
            EmailConfirmed = loginResp.EmailConfirmed,
        };

        try
        {
            await _js.InvokeVoidAsync("localStorage.setItem", TokenKey, _cachedToken);
            await _js.InvokeVoidAsync("localStorage.setItem",
                UserKey, JsonSerializer.Serialize(_cachedUser));
        }
        catch { }

        AuthStateChanged?.Invoke();
    }

    private static async Task<string> TryGetError(HttpResponseMessage response)
    {
        try
        {
            var content = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(content))
                return response.ReasonPhrase ?? "Error desconocido.";

            var formatted = FormatApiError(content);
            if (!string.IsNullOrWhiteSpace(formatted))
                return formatted;

            return response.ReasonPhrase ?? "Error desconocido.";
        }
        catch
        {
            return response.ReasonPhrase ?? "Error desconocido.";
        }
    }

    private static string? FormatApiError(string content)
    {
        var trimmed = content.Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
            return null;

        try
        {
            using var doc = JsonDocument.Parse(trimmed);
            var root = doc.RootElement;

            if (root.TryGetProperty("message", out var messageElement)
                && messageElement.ValueKind == JsonValueKind.String)
            {
                return TranslateGeneralMessage(messageElement.GetString());
            }

            if (root.TryGetProperty("errors", out var errorsElement)
                && errorsElement.ValueKind == JsonValueKind.Object)
            {
                var validationMessages = BuildValidationMessages(errorsElement);
                if (validationMessages.Count > 0)
                    return string.Join("\n", validationMessages);

                return "Hay errores de validación. Revisa los datos e inténtalo nuevamente.";
            }

            if (root.TryGetProperty("title", out var titleElement)
                && titleElement.ValueKind == JsonValueKind.String)
            {
                var translatedTitle = TranslateGeneralMessage(titleElement.GetString());
                if (!string.IsNullOrWhiteSpace(translatedTitle))
                    return translatedTitle;
            }

            return TranslateGeneralMessage(trimmed);
        }
        catch
        {
            return TranslateGeneralMessage(trimmed);
        }
    }

    private static List<string> BuildValidationMessages(JsonElement errorsElement)
    {
        var messages = new List<string>();

        foreach (var field in errorsElement.EnumerateObject())
        {
            var fieldNameEs = TranslateFieldName(field.Name);
            if (field.Value.ValueKind != JsonValueKind.Array)
                continue;

            foreach (var item in field.Value.EnumerateArray())
            {
                if (item.ValueKind != JsonValueKind.String)
                    continue;

                var raw = item.GetString();
                if (string.IsNullOrWhiteSpace(raw))
                    continue;

                var translated = TranslateValidationMessage(raw, fieldNameEs);
                if (!string.IsNullOrWhiteSpace(translated))
                    messages.Add($"- {translated}");
            }
        }

        return messages.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
    }

    private static string TranslateValidationMessage(string rawMessage, string fieldNameEs)
    {
        var message = rawMessage.Trim();

        if (message.Contains("minimum length", StringComparison.OrdinalIgnoreCase))
        {
            var match = Regex.Match(message, @"minimum length of '?(\d+)'?");
            if (match.Success)
                return $"El campo {fieldNameEs} debe tener al menos {match.Groups[1].Value} caracteres.";

            return $"El campo {fieldNameEs} no cumple con la longitud mínima requerida.";
        }

        if (message.Contains("maximum length", StringComparison.OrdinalIgnoreCase))
        {
            var match = Regex.Match(message, @"maximum length of '?(\d+)'?");
            if (match.Success)
                return $"El campo {fieldNameEs} permite como máximo {match.Groups[1].Value} caracteres.";

            return $"El campo {fieldNameEs} excede la longitud máxima permitida.";
        }

        if (message.Contains("is required", StringComparison.OrdinalIgnoreCase))
            return $"El campo {fieldNameEs} es obligatorio.";

        if (message.Contains("not a valid e-mail", StringComparison.OrdinalIgnoreCase)
            || message.Contains("must be a valid email", StringComparison.OrdinalIgnoreCase))
        {
            return "El correo electrónico no tiene un formato válido.";
        }

        return TranslateGeneralMessage(message)
               .Replace("Password", "Contraseña", StringComparison.OrdinalIgnoreCase)
               .Replace("Email", "Correo electrónico", StringComparison.OrdinalIgnoreCase);
    }

    private static string TranslateGeneralMessage(string? message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return "Error desconocido.";

        var value = message.Trim().Trim('"');

        if (value.Equals("One or more validation errors occurred.", StringComparison.OrdinalIgnoreCase))
            return "Hay errores de validación. Revisa los datos e inténtalo nuevamente.";

        if (value.Contains("Passwords must be at least", StringComparison.OrdinalIgnoreCase))
        {
            var match = Regex.Match(value, @"at least\s+(\d+)");
            if (match.Success)
                return $"La contraseña debe tener al menos {match.Groups[1].Value} caracteres.";

            return "La contraseña no cumple con la longitud mínima requerida.";
        }

        if (value.Contains("Passwords must have at least one non alphanumeric", StringComparison.OrdinalIgnoreCase))
            return "La contraseña debe incluir al menos un carácter especial.";

        if (value.Contains("Passwords must have at least one digit", StringComparison.OrdinalIgnoreCase))
            return "La contraseña debe incluir al menos un número.";

        if (value.Contains("Passwords must have at least one uppercase", StringComparison.OrdinalIgnoreCase))
            return "La contraseña debe incluir al menos una letra mayúscula.";

        if (value.Contains("Passwords must have at least one lowercase", StringComparison.OrdinalIgnoreCase))
            return "La contraseña debe incluir al menos una letra minúscula.";

        return value;
    }

    private static string TranslateFieldName(string fieldName)
    {
        return fieldName switch
        {
            "FirstName" => "Nombre",
            "LastName" => "Apellido",
            "Email" => "Correo electrónico",
            "Password" => "Contraseña",
            "ConfirmPassword" => "Confirmación de contraseña",
            "CurrentPassword" => "Contraseña actual",
            "NewPassword" => "Nueva contraseña",
            "InstitutionId" => "Institución",
            _ => fieldName,
        };
    }
}
