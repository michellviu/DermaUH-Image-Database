using System.Net.Http.Json;
using Microsoft.AspNetCore.Components;
using Web.DermaImage.Shared.Models;

namespace Web.DermaImage.Components.Pages.Users;

public partial class UserList
{
    private const int PageSize = 10;

    private PagedResponse<UserDto>? response;
    private bool loading = true;
    private bool showForm;
    private bool submitting;
    private int currentPage = 1;
    private string? errorMessage;
    private UserFormModel newUser = new();
    private readonly Dictionary<Guid, List<string>> userRoles = new();
    private readonly Dictionary<Guid, string> selectedAssignRoles = new();
    private readonly Dictionary<Guid, string> selectedRemoveRoles = new();
    private readonly string[] assignableRoles = ["Viewer", "Contributor", "Reviewer", "Admin"];

    protected override async Task OnInitializedAsync()
    {
        await LoadData(1);
    }

    private async Task LoadData(int page)
    {
        loading = true;
        currentPage = Math.Max(1, page);

        try
        {
            response = await Http.GetFromJsonAsync<PagedResponse<UserDto>>($"api/users?page={currentPage}&pageSize={PageSize}");
            userRoles.Clear();
            selectedAssignRoles.Clear();
            selectedRemoveRoles.Clear();

            if (response?.Items is not null)
            {
                var roleTasks = response.Items.Select(async user =>
                {
                    try
                    {
                        var roles = await Http.GetFromJsonAsync<List<string>>($"api/users/{user.Id}/roles");
                        return new { user.Id, Roles = roles ?? [] };
                    }
                    catch
                    {
                        return new { user.Id, Roles = new List<string>() };
                    }
                });

                var roleResults = await Task.WhenAll(roleTasks);
                foreach (var result in roleResults)
                {
                    userRoles[result.Id] = result.Roles
                        .Where(r => !string.IsNullOrWhiteSpace(r))
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .ToList();
                }

                foreach (var user in response.Items)
                {
                    var rolesForUser = GetRolesForUser(user);
                    if (rolesForUser.Count > 0)
                    {
                        selectedRemoveRoles[user.Id] = rolesForUser[0];
                    }

                    var assignableForUser = GetAssignableRolesForUser(user);
                    if (assignableForUser.Count > 0)
                    {
                        selectedAssignRoles[user.Id] = assignableForUser[0];
                    }
                }
            }
        }
        catch
        {
            response = null;
        }

        loading = false;
    }

    private async Task HandleCreate()
    {
        submitting = true;
        errorMessage = null;

        try
        {
            var result = await Http.PostAsJsonAsync("api/users", new
            {
                newUser.FirstName,
                newUser.LastName,
                newUser.Email,
                newUser.Password,
            });

            if (result.IsSuccessStatusCode)
            {
                newUser = new();
                showForm = false;
                await LoadData(1);
            }
            else
            {
                errorMessage = $"Error: {result.StatusCode}";
            }
        }
        catch (Exception ex)
        {
            errorMessage = ex.Message;
        }
        finally
        {
            submitting = false;
        }
    }

    private List<string> GetRolesForUser(UserDto user)
    {
        if (userRoles.TryGetValue(user.Id, out var roles) && roles.Count > 0)
        {
            return roles;
        }

        return string.IsNullOrWhiteSpace(user.Role)
            ? []
            : [user.Role];
    }

    private List<string> GetAssignableRolesForUser(UserDto user)
    {
        var existingRoles = GetRolesForUser(user);
        return assignableRoles
            .Where(role => !existingRoles.Contains(role, StringComparer.OrdinalIgnoreCase))
            .ToList();
    }

    private string GetSelectedAssignRole(Guid userId)
    {
        return selectedAssignRoles.TryGetValue(userId, out var role)
            ? role
            : string.Empty;
    }

    private void OnAssignRoleSelectionChanged(Guid userId, ChangeEventArgs e)
    {
        var role = e.Value?.ToString();
        if (string.IsNullOrWhiteSpace(role))
        {
            return;
        }

        selectedAssignRoles[userId] = role;
    }

    private string GetSelectedRemoveRole(Guid userId)
    {
        return selectedRemoveRoles.TryGetValue(userId, out var role)
            ? role
            : string.Empty;
    }

    private void OnRemoveRoleSelectionChanged(Guid userId, ChangeEventArgs e)
    {
        var role = e.Value?.ToString();
        if (string.IsNullOrWhiteSpace(role))
        {
            return;
        }

        selectedRemoveRoles[userId] = role;
    }

    private async Task HandleAssignRole(Guid userId)
    {
        errorMessage = null;
        var role = GetSelectedAssignRole(userId);
        var currentRoles = response?.Items
            .Where(x => x.Id == userId)
            .SelectMany(GetRolesForUser)
            .ToList() ?? [];

        if (string.IsNullOrWhiteSpace(role))
        {
            errorMessage = "Seleccione un rol para asignar.";
            return;
        }

        if (currentRoles.Contains(role, StringComparer.OrdinalIgnoreCase))
        {
            errorMessage = "El usuario ya tiene este rol asignado.";
            return;
        }

        try
        {
            var result = await Http.PostAsJsonAsync($"api/users/{userId}/roles", new AssignRoleRequest { Role = role });
            if (!result.IsSuccessStatusCode)
            {
                var detail = await result.Content.ReadAsStringAsync();
                errorMessage = string.IsNullOrWhiteSpace(detail)
                    ? $"Error al asignar rol: {result.StatusCode}"
                    : $"Error al asignar rol: {detail}";
                return;
            }

            await LoadData(currentPage);
        }
        catch (Exception ex)
        {
            errorMessage = ex.Message;
        }
    }

    private async Task HandleRemoveRole(Guid userId)
    {
        errorMessage = null;
        var role = GetSelectedRemoveRole(userId);

        if (string.IsNullOrWhiteSpace(role))
        {
            errorMessage = "Seleccione un rol para quitar.";
            return;
        }

        try
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, $"api/users/{userId}/roles")
            {
                Content = JsonContent.Create(new AssignRoleRequest { Role = role }),
            };

            var result = await Http.SendAsync(request);
            if (!result.IsSuccessStatusCode)
            {
                var detail = await result.Content.ReadAsStringAsync();
                errorMessage = string.IsNullOrWhiteSpace(detail)
                    ? $"Error al quitar rol: {result.StatusCode}"
                    : $"Error al quitar rol: {detail}";
                return;
            }

            await LoadData(currentPage);
        }
        catch (Exception ex)
        {
            errorMessage = ex.Message;
        }
    }

    private Task GoToPreviousPageAsync()
    {
        if (response?.HasPrevious != true)
        {
            return Task.CompletedTask;
        }

        return LoadData(currentPage - 1);
    }

    private Task GoToNextPageAsync()
    {
        if (response?.HasNext != true)
        {
            return Task.CompletedTask;
        }

        return LoadData(currentPage + 1);
    }

    private class UserFormModel
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
