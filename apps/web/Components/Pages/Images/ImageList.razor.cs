using System.Net.Http.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using Web.DermaImage.Shared.Models;

namespace Web.DermaImage.Components.Pages.Images;

public partial class ImageList
{
    [CascadingParameter] private Task<AuthenticationState>? AuthenticationStateTask { get; set; }

    private List<DermaImgDto> images = [];
    private bool hasLoaded;
    private int totalCount;
    private string? loadingError;
    private bool isLoadingPage;
    private bool hasMorePages = true;
    private int nextPageToLoad = 1;
    private int diagnosisInputVersion;
    private ElementReference diagnosisInputRef;
    private readonly string sentinelId = $"images-infinite-sentinel-{Guid.NewGuid():N}";
    private DotNetObjectReference<ImageList>? dotNetRef;
    private bool observerInitialized;

    private const int DefaultPageSize = 10;

    private string? diagnosisContains;
    private bool onlyMyContributions;
    private bool isAuthenticated;
    private Guid? currentUserId;

    private readonly HashSet<string> selectedImageTypes = [];
    private readonly HashSet<string> selectedDiagnosisCategories = [];
    private readonly HashSet<string> selectedInjuryTypes = [];
    private readonly HashSet<string> selectedFotoTypes = [];
    private readonly HashSet<string> selectedSexes = [];
    private readonly HashSet<string> selectedAnatomSites = [];

    private static readonly IReadOnlyList<FilterOption> imageTypeOptions =
    [
        new("Dermoscopic", "Dermoscópica"),
        new("ClinicalOverview", "Clínica: Vista General"),
        new("ClinicalCloseUp", "Clínica: Primer Plano"),
        new("TBPTileOverview", "TBP: Vista General"),
        new("TBPTileCloseUp", "TBP: Primer Plano"),
        new("RCMMacroscopic", "RCM: Macroscópica"),
        new("RCMTile", "RCM: Tile"),
        new("RCMMosaic", "RCM: Mosaico"),
    ];

    private static readonly IReadOnlyList<FilterOption> diagnosisCategoryOptions =
    [
        new("Benign", "Benigno"),
        new("Indeterminate", "Indeterminado"),
        new("Malignant", "Maligno"),
    ];

    private static readonly IReadOnlyList<FilterOption> injuryTypeOptions =
    [
        new("Melanoma", "Melanoma"),
        new("BasalCellCarcinoma", "Carcinoma Basocelular"),
        new("SquamousCellCarcinoma", "Carcinoma Escamocelular"),
        new("Others", "Otros"),
    ];

    private static readonly IReadOnlyList<FilterOption> photoTypeOptions =
    [
        new("I", "I"),
        new("II", "II"),
        new("III", "III"),
        new("IV", "IV"),
        new("V", "V"),
        new("VI", "VI"),
    ];

    private static readonly IReadOnlyList<FilterOption> sexOptions =
    [
        new("Male", "Masculino"),
        new("Female", "Femenino"),
    ];

    private static readonly IReadOnlyList<FilterOption> anatomSiteOptions =
    [
        new("HeadNeck", "Cabeza/Cuello"),
        new("UpperExtremity", "Extremidad Superior"),
        new("LowerExtremity", "Extremidad Inferior"),
        new("AnteriorTorso", "Torso Anterior"),
        new("LateralTorso", "Torso Lateral"),
        new("PosteriorTorso", "Torso Posterior"),
        new("PalmsSoles", "Palmas/Plantas"),
        new("OralGenital", "Oral/Genital"),
    ];

    protected override async Task OnInitializedAsync()
    {
        await ResolveUserContextAsync();
        await ResetAndLoadImagesAsync();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!observerInitialized)
        {
            dotNetRef = DotNetObjectReference.Create(this);
            await JS.InvokeVoidAsync("infiniteScroll.init", dotNetRef, sentinelId);
            observerInitialized = true;
        }

        if (observerInitialized && hasMorePages && !isLoadingPage)
        {
            await JS.InvokeVoidAsync("infiniteScroll.check", sentinelId);
        }
    }

    private async Task ApplyFiltersAsync()
    {
        await ResetAndLoadImagesAsync();
    }

    private async Task OnDiagnosisInputChangedAsync(ChangeEventArgs args)
    {
        diagnosisContains = args.Value?.ToString();
        var version = ++diagnosisInputVersion;
        await Task.Delay(350);

        if (version != diagnosisInputVersion)
        {
            return;
        }

        await ApplyFiltersAsync();
    }

    private async Task ClearFiltersAsync()
    {
        diagnosisContains = null;
        diagnosisInputVersion++;
        onlyMyContributions = false;
        selectedImageTypes.Clear();
        selectedDiagnosisCategories.Clear();
        selectedInjuryTypes.Clear();
        selectedFotoTypes.Clear();
        selectedSexes.Clear();
        selectedAnatomSites.Clear();

        await ResetAndLoadImagesAsync();
        await diagnosisInputRef.FocusAsync();
    }

    private async Task ToggleOnlyMyContributionsAsync(ChangeEventArgs args)
    {
        onlyMyContributions = args.Value as bool? ?? false;
        await ApplyFiltersAsync();
    }

    private async Task ResolveUserContextAsync()
    {
        if (AuthenticationStateTask is null)
        {
            return;
        }

        var authState = await AuthenticationStateTask;
        var user = authState.User;
        isAuthenticated = user.Identity?.IsAuthenticated ?? false;

        var idValue = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? user.FindFirst("sub")?.Value
            ?? user.FindFirst(ClaimTypes.Name)?.Value;

        if (Guid.TryParse(idValue, out var parsedId))
        {
            currentUserId = parsedId;
        }
    }

    private async Task ResetAndLoadImagesAsync()
    {
        images = [];
        totalCount = 0;
        nextPageToLoad = 1;
        hasMorePages = true;
        loadingError = null;
        hasLoaded = false;

        await LoadNextPageAsync();
    }

    [JSInvokable]
    public async Task OnInfiniteScrollTrigger()
    {
        await LoadNextPageAsync();
    }

    private async Task LoadMoreAsync()
    {
        await LoadNextPageAsync();
    }

    private async Task LoadNextPageAsync()
    {
        if (isLoadingPage || !hasMorePages)
        {
            return;
        }

        isLoadingPage = true;

        try
        {
            var query = BuildQuery(nextPageToLoad, DefaultPageSize);
            var response = await Http.GetFromJsonAsync<PagedResponse<DermaImgDto>>(query);
            if (response is null)
            {
                hasLoaded = true;
                loadingError = "No se pudo obtener la respuesta del servidor.";
                hasMorePages = false;
                return;
            }

            totalCount = response.TotalCount;
            images.AddRange(response.Items);
            hasLoaded = true;
            loadingError = null;
            hasMorePages = response.HasNext;
            nextPageToLoad = response.Page + 1;
        }
        catch
        {
            if (!hasLoaded)
            {
                totalCount = 0;
                images = [];
                hasLoaded = true;
                loadingError = "Error al cargar imágenes.";
            }

            hasMorePages = false;
        }
        finally
        {
            isLoadingPage = false;
        }
    }

    private string BuildQuery(int page, int pageSize)
    {
        var queryParams = new List<string>
        {
            $"page={page}",
            $"pageSize={pageSize}",
        };

        foreach (var value in selectedImageTypes)
        {
            queryParams.Add($"imageTypes={Uri.EscapeDataString(value)}");
        }

        foreach (var value in selectedDiagnosisCategories)
        {
            queryParams.Add($"diagnosisCategories={Uri.EscapeDataString(value)}");
        }

        foreach (var value in selectedInjuryTypes)
        {
            queryParams.Add($"injuryTypes={Uri.EscapeDataString(value)}");
        }

        foreach (var value in selectedFotoTypes)
        {
            queryParams.Add($"fotoTypes={Uri.EscapeDataString(value)}");
        }

        foreach (var value in selectedSexes)
        {
            queryParams.Add($"sexes={Uri.EscapeDataString(value)}");
        }

        foreach (var value in selectedAnatomSites)
        {
            queryParams.Add($"anatomSites={Uri.EscapeDataString(value)}");
        }

        if (!string.IsNullOrWhiteSpace(diagnosisContains))
        {
            queryParams.Add($"diagnosisContains={Uri.EscapeDataString(diagnosisContains.Trim())}");
        }

        if (onlyMyContributions && currentUserId.HasValue)
        {
            queryParams.Add($"contributorId={currentUserId.Value}");
        }

        queryParams.Add("isPublic=true");

        return $"api/images?{string.Join("&", queryParams)}";
    }

    private async Task ToggleSelection(ChangeEventArgs args, string value, HashSet<string> selectedValues)
    {
        var isChecked = args.Value as bool? ?? false;
        if (isChecked)
        {
            selectedValues.Add(value);
        }
        else
        {
            selectedValues.Remove(value);
        }

        await ApplyFiltersAsync();
    }

    public async ValueTask DisposeAsync()
    {
        if (observerInitialized)
        {
            try
            {
                await JS.InvokeVoidAsync("infiniteScroll.dispose", sentinelId);
            }
            catch
            {
                // Ignore disposal errors when circuit is closing.
            }
        }

        dotNetRef?.Dispose();
    }

    private sealed record FilterOption(string Value, string Label);
}
