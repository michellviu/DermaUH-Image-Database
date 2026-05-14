using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using Web.DermaImage.Services;
using Web.DermaImage.Shared.Models;

namespace Web.DermaImage.Components.Pages.Images;

public partial class ImageList
{
    [CascadingParameter] private Task<AuthenticationState>? AuthenticationStateTask { get; set; }

    private List<DermaImgDto> images = [];
    private bool hasLoaded;
    private int totalCount;
    private string? loadingError;
    private string? downloadError;
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
    private bool isDownloading;
    private ImageDownloadMode downloadMode = ImageDownloadMode.ImagesAndMetadata;

    private readonly HashSet<Guid> selectedImageIds = [];

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
        downloadError = null;
        selectedImageIds.Clear();
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
        var queryParams = BuildFilterQueryParams();
        queryParams.Insert(0, $"pageSize={pageSize}");
        queryParams.Insert(0, $"page={page}");

        return $"api/images?{string.Join("&", queryParams)}";
    }

    private List<string> BuildFilterQueryParams()
    {
        var queryParams = new List<string>();

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

        return queryParams;
    }

    private string BuildDownloadAllQuery()
    {
        var queryParams = BuildFilterQueryParams();
        queryParams.Add(BuildDownloadModeQuery());
        return $"api/images/download?{string.Join("&", queryParams)}";
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

    private bool IsImageSelected(Guid id) => selectedImageIds.Contains(id);

    private bool IsPageFullySelected => images.Count > 0 && images.All(img => selectedImageIds.Contains(img.Id));

    private Task ToggleImageSelection(ChangeEventArgs args, Guid id)
    {
        var isChecked = args.Value as bool? ?? false;
        if (isChecked)
        {
            selectedImageIds.Add(id);
        }
        else
        {
            selectedImageIds.Remove(id);
        }

        return Task.CompletedTask;
    }

    private Task ToggleSelectAllOnPage(ChangeEventArgs args)
    {
        var isChecked = args.Value as bool? ?? false;

        if (isChecked)
        {
            foreach (var image in images)
            {
                selectedImageIds.Add(image.Id);
            }
        }
        else
        {
            foreach (var image in images)
            {
                selectedImageIds.Remove(image.Id);
            }
        }

        return Task.CompletedTask;
    }

    private async Task DownloadSelectedAsync()
    {
        if (selectedImageIds.Count == 0)
        {
            return;
        }

        var payload = new DownloadImagesRequest
        {
            ImageIds = selectedImageIds.ToList()
        };

        var request = new HttpRequestMessage(HttpMethod.Post, $"api/images/download?{BuildDownloadModeQuery()}")
        {
            Content = JsonContent.Create(payload)
        };

        var fallbackName = BuildDownloadFallbackName("seleccion");
        await StartDownloadAsync(request, fallbackName);
    }

    private async Task DownloadAllAsync()
    {
        if (totalCount == 0)
        {
            return;
        }

        var request = new HttpRequestMessage(HttpMethod.Get, BuildDownloadAllQuery());
        var fallbackName = BuildDownloadFallbackName("galeria");
        await StartDownloadAsync(request, fallbackName);
    }

    private string BuildDownloadModeQuery()
    {
        return $"mode={downloadMode}";
    }

    private string BuildDownloadFallbackName(string scope)
    {
        var modeSuffix = downloadMode switch
        {
            ImageDownloadMode.MetadataOnly => "metadatos",
            ImageDownloadMode.ImagesOnly => "solo-imagenes",
            _ => "imagenes-metadatos"
        };

        return $"dermauh-imagenes-{scope}-{modeSuffix}-{DateTime.UtcNow:yyyyMMdd-HHmm}.zip";
    }

    private async Task StartDownloadAsync(HttpRequestMessage request, string fallbackName)
    {
        if (isDownloading)
        {
            return;
        }

        isDownloading = true;
        downloadError = null;

        try
        {
            using var response = await Http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            if (!response.IsSuccessStatusCode)
            {
                downloadError = await ApiValidationMessageParser.BuildFriendlyErrorMessageAsync(response);
                return;
            }

            var fileName = ResolveFileName(response, fallbackName);
            await using var stream = await response.Content.ReadAsStreamAsync();
            using var streamRef = new DotNetStreamReference(stream);
            await JS.InvokeVoidAsync("fileDownloads.downloadFileFromStream", fileName, streamRef);
        }
        catch (Exception ex)
        {
            downloadError = $"No fue posible descargar las imagenes: {ex.Message}";
        }
        finally
        {
            isDownloading = false;
        }
    }

    private static string ResolveFileName(HttpResponseMessage response, string fallback)
    {
        var contentDisposition = response.Content.Headers.ContentDisposition;
        var candidate = contentDisposition?.FileNameStar ?? contentDisposition?.FileName;
        return string.IsNullOrWhiteSpace(candidate) ? fallback : candidate.Trim('"');
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

    private enum ImageDownloadMode
    {
        ImagesAndMetadata,
        MetadataOnly,
        ImagesOnly
    }
}
