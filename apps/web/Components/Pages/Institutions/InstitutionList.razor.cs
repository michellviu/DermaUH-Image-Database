using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Web.DermaImage.Shared.Models;

namespace Web.DermaImage.Components.Pages.Institutions;

public partial class InstitutionList : IAsyncDisposable
{
    private const string CarouselRegionId = "institutions-carousel-region";
    private const string CarouselTrackId = "institutions-carousel-track";
    private List<InstitutionDto>? institutions;
    private bool loading = true;
    private bool carouselInitialized;

    [Inject] private IJSRuntime JS { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        await LoadData();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (loading || institutions is null || institutions.Count < 2)
        {
            return;
        }

        if (!carouselInitialized)
        {
            carouselInitialized = true;
            await JS.InvokeVoidAsync("institutionsCarousel.init", CarouselRegionId, CarouselTrackId);
            return;
        }

        await JS.InvokeVoidAsync("institutionsCarousel.recalculate", CarouselRegionId);
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

    public async ValueTask DisposeAsync()
    {
        if (!carouselInitialized)
        {
            return;
        }

        try
        {
            await JS.InvokeVoidAsync("institutionsCarousel.dispose", CarouselRegionId);
        }
        catch
        {
            // Ignore if JS runtime is unavailable during disposal.
        }
    }
}
