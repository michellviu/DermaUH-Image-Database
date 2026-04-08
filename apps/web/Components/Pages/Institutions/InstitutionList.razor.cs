using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Web.DermaImage.Shared.Models;

namespace Web.DermaImage.Components.Pages.Institutions;

public partial class InstitutionList
{
    private const int PageSize = 10;

    private PagedResponse<InstitutionDto>? response;
    private bool loading = true;
    private bool showForm;
    private bool submitting;
    private string? errorMessage;
    private InstitutionFormModel newInstitution = new();
    private InstitutionFormModel editingInstitution = new();
    private Guid? editingInstitutionId;
    private Guid? updatingInstitutionId;
    private List<UserDto> allUsers = new();
    private readonly Dictionary<Guid, List<InstitutionResponsibleDto>> institutionResponsibles = new();
    private readonly Dictionary<Guid, Guid> selectedResponsibleUsers = new();
    private readonly string carouselRegionId = $"institutions-carousel-region-{Guid.NewGuid():N}";
    private readonly string carouselTrackId = $"institutions-carousel-track-{Guid.NewGuid():N}";
    private bool carouselInitialized;

    protected override async Task OnInitializedAsync()
    {
        await LoadData();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (carouselInitialized)
        {
            return;
        }

        if (loading || response?.Items is null || response.Items.Count() < 2)
        {
            return;
        }

        await JS.InvokeVoidAsync("institutionsCarousel.init", carouselRegionId, carouselTrackId);
        carouselInitialized = true;
    }

    private async Task LoadData()
    {
        loading = true;
        try
        {
            var allInstitutions = new List<InstitutionDto>();
            var page = 1;
            var totalPages = 1;
            var totalCount = 0;

            do
            {
                var pageResponse = await Http.GetFromJsonAsync<PagedResponse<InstitutionDto>>($"api/institutions?page={page}&pageSize={PageSize}");
                if (pageResponse is null)
                {
                    break;
                }

                allInstitutions.AddRange(pageResponse.Items);
                totalCount = pageResponse.TotalCount;
                totalPages = Math.Max(1, pageResponse.TotalPages);
                page++;
            }
            while (page <= totalPages);

            response = new PagedResponse<InstitutionDto>
            {
                Items = allInstitutions,
                TotalCount = totalCount,
                Page = 1,
                PageSize = PageSize,
                TotalPages = Math.Max(1, totalPages),
                HasNext = false,
                HasPrevious = false,
            };

            await LoadResponsiblesData();
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
            var result = await Http.PostAsJsonAsync("api/institutions", new
            {
                newInstitution.Name,
                newInstitution.Description,
                newInstitution.Country,
                newInstitution.City,
                newInstitution.Address,
                newInstitution.Website,
                newInstitution.ContactEmail,
            });

            if (result.IsSuccessStatusCode)
            {
                newInstitution = new();
                showForm = false;
                await LoadData();

                if (carouselInitialized)
                {
                    await JS.InvokeVoidAsync("institutionsCarousel.recalculate", carouselRegionId, carouselTrackId);
                }
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

    private void BeginEditInstitution(InstitutionDto institution)
    {
        errorMessage = null;
        editingInstitutionId = institution.Id;
        editingInstitution = new InstitutionFormModel
        {
            Name = institution.Name,
            Description = institution.Description,
            Country = institution.Country,
            City = institution.City,
            Address = institution.Address,
            Website = institution.Website,
            ContactEmail = institution.ContactEmail,
        };
    }

    private void CancelEditInstitution()
    {
        editingInstitutionId = null;
        updatingInstitutionId = null;
        editingInstitution = new InstitutionFormModel();
    }

    private async Task HandleUpdateInstitution(Guid institutionId)
    {
        if (editingInstitutionId != institutionId)
        {
            return;
        }

        updatingInstitutionId = institutionId;
        errorMessage = null;

        try
        {
            var result = await Http.PutAsJsonAsync($"api/institutions/{institutionId}", new
            {
                editingInstitution.Name,
                editingInstitution.Description,
                editingInstitution.Country,
                editingInstitution.City,
                editingInstitution.Address,
                editingInstitution.Website,
                editingInstitution.ContactEmail,
            });

            if (!result.IsSuccessStatusCode)
            {
                errorMessage = await GetApiErrorAsync(result);
                return;
            }

            await LoadData();
            CancelEditInstitution();

            if (carouselInitialized)
            {
                await JS.InvokeVoidAsync("institutionsCarousel.recalculate", carouselRegionId, carouselTrackId);
            }
        }
        catch (Exception ex)
        {
            errorMessage = ex.Message;
        }
        finally
        {
            updatingInstitutionId = null;
        }
    }

    private async Task LoadResponsiblesData()
    {
        institutionResponsibles.Clear();
        selectedResponsibleUsers.Clear();
        allUsers = await LoadAllUsersAsync();

        if (response?.Items is null || !response.Items.Any())
        {
            return;
        }

        foreach (var institution in response.Items)
        {
            await RefreshInstitutionResponsibles(institution.Id);
        }
    }

    private async Task<List<UserDto>> LoadAllUsersAsync()
    {
        var users = new List<UserDto>();
        var page = 1;
        var totalPages = 1;

        try
        {
            do
            {
                var pageResponse = await Http.GetFromJsonAsync<PagedResponse<UserDto>>($"api/users?page={page}&pageSize={PageSize}");
                if (pageResponse is null)
                {
                    break;
                }

                users.AddRange(pageResponse.Items);
                totalPages = Math.Max(1, pageResponse.TotalPages);
                page++;
            }
            while (page <= totalPages);
        }
        catch
        {
            // Non-admin users won't have access to users endpoint.
            return [];
        }

        return users;
    }

    private async Task RefreshInstitutionResponsibles(Guid institutionId)
    {
        try
        {
            var responsibles = await Http.GetFromJsonAsync<List<InstitutionResponsibleDto>>($"api/institutions/{institutionId}/responsibles");
            institutionResponsibles[institutionId] = responsibles ?? [];
        }
        catch
        {
            institutionResponsibles[institutionId] = [];
        }

        var candidates = GetAvailableResponsibleCandidates(institutionId);
        if (candidates.Count > 0)
        {
            selectedResponsibleUsers[institutionId] = candidates[0].Id;
        }
        else
        {
            selectedResponsibleUsers.Remove(institutionId);
        }
    }

    private List<InstitutionResponsibleDto> GetResponsiblesForInstitution(Guid institutionId)
    {
        return institutionResponsibles.TryGetValue(institutionId, out var responsibles)
            ? responsibles
            : [];
    }

    private List<UserDto> GetAvailableResponsibleCandidates(Guid institutionId)
    {
        var alreadyResponsible = GetResponsiblesForInstitution(institutionId)
            .Select(x => x.UserId)
            .ToHashSet();

        return allUsers
            .Where(u => !alreadyResponsible.Contains(u.Id))
            .OrderBy(u => u.LastName)
            .ThenBy(u => u.FirstName)
            .ToList();
    }

    private string GetSelectedResponsibleUser(Guid institutionId)
    {
        return selectedResponsibleUsers.TryGetValue(institutionId, out var selected)
            ? selected.ToString()
            : string.Empty;
    }

    private void OnResponsibleSelectionChanged(Guid institutionId, ChangeEventArgs e)
    {
        var value = e.Value?.ToString();
        if (!Guid.TryParse(value, out var userId))
        {
            return;
        }

        selectedResponsibleUsers[institutionId] = userId;
    }

    private async Task HandleAssignResponsible(Guid institutionId)
    {
        errorMessage = null;

        if (!selectedResponsibleUsers.TryGetValue(institutionId, out var userId))
        {
            errorMessage = "Seleccione un usuario para asignar como responsable.";
            return;
        }

        try
        {
            var result = await Http.PostAsJsonAsync($"api/institutions/{institutionId}/responsibles", new { UserId = userId });
            if (!result.IsSuccessStatusCode)
            {
                errorMessage = await GetApiErrorAsync(result);
                return;
            }

            await RefreshInstitutionResponsibles(institutionId);
        }
        catch (Exception ex)
        {
            errorMessage = ex.Message;
        }
    }

    private async Task HandleRemoveResponsible(Guid institutionId, Guid userId)
    {
        errorMessage = null;

        try
        {
            var result = await Http.DeleteAsync($"api/institutions/{institutionId}/responsibles/{userId}");
            if (!result.IsSuccessStatusCode)
            {
                errorMessage = await GetApiErrorAsync(result);
                return;
            }

            await RefreshInstitutionResponsibles(institutionId);
        }
        catch (Exception ex)
        {
            errorMessage = ex.Message;
        }
    }

    private static async Task<string> GetApiErrorAsync(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        if (string.IsNullOrWhiteSpace(content))
        {
            return $"Error: {response.StatusCode}";
        }

        try
        {
            using var json = JsonDocument.Parse(content);
            if (json.RootElement.TryGetProperty("message", out var msg)
                && msg.ValueKind == JsonValueKind.String)
            {
                return msg.GetString() ?? $"Error: {response.StatusCode}";
            }
        }
        catch
        {
            // Ignore parsing errors.
        }

        return $"Error: {response.StatusCode}";
    }

    private class InstitutionFormModel
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Country { get; set; }
        public string? City { get; set; }
        public string? Address { get; set; }
        public string? Website { get; set; }
        public string? ContactEmail { get; set; }
    }

    public async ValueTask DisposeAsync()
    {
        if (!carouselInitialized)
        {
            return;
        }

        try
        {
            await JS.InvokeVoidAsync("institutionsCarousel.dispose", carouselRegionId);
        }
        catch
        {
            // Ignore disposal errors when leaving the page.
        }
    }
}
