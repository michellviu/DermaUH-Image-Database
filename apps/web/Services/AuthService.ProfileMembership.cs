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

    public async Task<(bool Success, string? Error)> CreateInstitutionJoinRequestAsync(Guid institutionId)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("/api/institutions/join-requests", new CreateInstitutionJoinRequest
            {
                InstitutionId = institutionId,
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
