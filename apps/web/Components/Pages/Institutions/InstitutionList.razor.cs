using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Web.DermaImage.Shared.Models;

namespace Web.DermaImage.Components.Pages.Institutions;

public partial class InstitutionList
{
    private List<InstitutionDto>? institutions;
    private bool loading = true;

    protected override async Task OnInitializedAsync()
    {
        await LoadData();
    }

    private async Task LoadData()
    {
        loading = true;
        try
        {
            institutions = await Http.GetFromJsonAsync<List<InstitutionDto>>("api/institutions");
        }
        catch
        {
            institutions = null;
        }

        loading = false;
    }
}
