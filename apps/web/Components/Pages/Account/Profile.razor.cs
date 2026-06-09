using System.Net.Http.Json;
using Web.DermaImage.Shared.Models;

namespace Web.DermaImage.Components.Pages.Account;

public partial class Profile
{
    private UserProfile? _profile;
    private UpdateProfileRequest _profileForm = new();
    private ChangePasswordRequest _pwdForm = new();

    private PagedResponse<UserDto>? _pendingUsersResponse;
    private bool _loadingPending;
    
    private int _pendingPage = 1;
    private int _pendingPageSize = 5;
    private string? _pendingEmailFilter;

    private bool _savingProfile;
    private bool _savingPwd;
    private bool _pwdMismatch;

    private string? _profileSuccess;
    private string? _profileError;
    private string? _pwdSuccess;
    private string? _pwdError;

    protected override async Task OnInitializedAsync()
    {
        await ReloadProfileAsync();

        if (_profile != null && _profile.Roles.Contains("Admin", StringComparer.OrdinalIgnoreCase))
        {
            await LoadPendingUsersAsync();
        }
    }

    private async Task LoadPendingUsersAsync()
    {
        _loadingPending = true;
        try
        {
            var url = $"api/users/pending?page={_pendingPage}&pageSize={_pendingPageSize}";
            if (!string.IsNullOrWhiteSpace(_pendingEmailFilter))
            {
                url += $"&emailFilter={Uri.EscapeDataString(_pendingEmailFilter)}";
            }
            _pendingUsersResponse = await Http.GetFromJsonAsync<PagedResponse<UserDto>>(url);
        }
        catch
        {
            _pendingUsersResponse = new PagedResponse<UserDto> { Items = new List<UserDto>() };
        }
        finally
        {
            _loadingPending = false;
            StateHasChanged();
        }
    }

    private async Task GoToPendingPreviousPageAsync()
    {
        if (_pendingUsersResponse != null && _pendingUsersResponse.HasPrevious)
        {
            _pendingPage--;
            await LoadPendingUsersAsync();
        }
    }

    private async Task GoToPendingNextPageAsync()
    {
        if (_pendingUsersResponse != null && _pendingUsersResponse.HasNext)
        {
            _pendingPage++;
            await LoadPendingUsersAsync();
        }
    }

    private async Task ApproveUserAsync(Guid id)
    {
        var response = await Http.PostAsync($"api/users/{id}/approve", null);
        if (response.IsSuccessStatusCode)
        {
            await LoadPendingUsersAsync();
        }
    }

    private async Task DenyUserAsync(Guid id)
    {
        var response = await Http.PostAsync($"api/users/{id}/deny", null);
        if (response.IsSuccessStatusCode)
        {
            await LoadPendingUsersAsync();
        }
    }

    private async Task SaveProfileAsync()
    {
        _savingProfile = true;
        _profileSuccess = null;
        _profileError = null;

        var (success, error) = await Auth.UpdateProfileAsync(_profileForm);

        _savingProfile = false;

        if (!success)
        {
            _profileError = error;
            return;
        }

        _profileSuccess = "Perfil actualizado correctamente.";
        await ReloadProfileAsync();
    }

    private async Task ChangePasswordAsync()
    {
        _pwdMismatch = false;
        _pwdSuccess = null;
        _pwdError = null;

        if (_pwdForm.NewPassword != _pwdForm.ConfirmPassword)
        {
            _pwdMismatch = true;
            return;
        }

        _savingPwd = true;
        var (success, error) = await Auth.ChangePasswordAsync(_pwdForm);
        _savingPwd = false;

        if (!success)
        {
            _pwdError = error;
            return;
        }

        _pwdSuccess = "Contraseña actualizada correctamente.";
        _pwdForm = new ChangePasswordRequest();
    }

    private async Task ReloadProfileAsync()
    {
        _profile = await Auth.FetchProfileAsync();

        if (_profile is null)
        {
            return;
        }

        _profileForm = new UpdateProfileRequest
        {
            FirstName = _profile.FirstName,
            LastName = _profile.LastName,
            PhoneNumber = _profile.PhoneNumber,
        };
    }
}
