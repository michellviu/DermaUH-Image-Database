using System.Net.Http.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Web.DermaImage.Services;
using Web.DermaImage.Shared.Models;

namespace Web.DermaImage.Components.Pages.Images;

public partial class ImageEdit
{
    [Parameter] public Guid Id { get; set; }

    [CascadingParameter] private Task<AuthenticationState>? AuthenticationStateTask { get; set; }

    private CreateImageFormModel model = new();
    private bool loading = true;
    private bool submitting;
    private string? loadError;
    private string? errorMessage;
    private string contributorInstitutionLabel = "Anónimo";

    private bool personalHxMm;
    private bool familyHxMm;
    private bool melUlcer;

    private bool dermoscopicEnabled => ImageCreateValidationRules.RequiresDermoscopicType(model.ImageType);
    private bool injuryTypeEnabled => ImageCreateValidationRules.AllowsInjuryType(model.DiagnosisCategory);
    private bool melanomaHistologyEnabled => ImageCreateValidationRules.AllowsMelanomaHistology(model.InjuryType);

    protected override async Task OnInitializedAsync()
    {
        await LoadAsync();
    }

    private async Task LoadAsync()
    {
        loading = true;
        loadError = null;

        try
        {
            var image = await Http.GetFromJsonAsync<DermaImgDto>($"api/images/{Id}");
            if (image is null)
            {
                loadError = "No se encontró la imagen solicitada.";
                return;
            }

            var authState = AuthenticationStateTask is null
                ? new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()))
                : await AuthenticationStateTask;

            if (!CanEditImage(authState.User, image))
            {
                loadError = "No es posible editar esta imagen con la sesión actual.";
                return;
            }

            MapFromImage(image);
            contributorInstitutionLabel = string.IsNullOrWhiteSpace(image.InstitutionName) ? "Anónimo" : image.InstitutionName;
            await OnDependencySourceChanged();
        }
        catch (Exception ex)
        {
            loadError = $"No fue posible cargar la edición de la imagen: {ex.Message}";
        }
        finally
        {
            loading = false;
        }
    }

    private void MapFromImage(DermaImgDto image)
    {
        model.FileName = image.FileName;
        model.FilePath = image.FilePath;
        model.ContentType = image.ContentType;
        model.FileSize = image.FileSize;
        model.IsPublic = image.IsPublic;
        model.ContributorId = image.ContributorId?.ToString();

        model.ImageType = image.ImageType;
        model.ImageManipulation = image.ImageManipulation;
        model.DermoscopicType = image.DermoscopicType;

        model.AgeApprox = image.AgeApprox;
        model.Sex = image.Sex;
        model.FotoType = image.FotoType;
        personalHxMm = image.PersonalHxMm == true;
        familyHxMm = image.FamilyHxMm == true;

        model.AnatomSiteGeneral = image.AnatomSiteGeneral;
        model.AnatomSiteSpecial = image.AnatomSiteSpecial;
        model.ClinSizeLongDiamMm = image.ClinSizeLongDiamMm;

        model.Diagnosis = image.Diagnosis;
        model.DiagnosisCategory = image.DiagnosisCategory;
        model.InjuryType = image.InjuryType;
        model.DiagnosisConfirmType = image.DiagnosisConfirmType;

        model.MelThickMm = image.MelThickMm;
        model.MelMitoticIndex = image.MelMitoticIndex;
        melUlcer = image.MelUlcer == true;

        model.ClinicalNotes = image.ClinicalNotes;
    }

    private Task OnDependencySourceChanged()
    {
        ImageCreateValidationRules.Normalize(model, ref melUlcer);
        return Task.CompletedTask;
    }

    private async Task HandleSubmit()
    {
        submitting = true;
        errorMessage = null;

        try
        {
            ImageCreateValidationRules.Normalize(model, ref melUlcer);

            var businessValidationErrors = ImageCreateValidationRules.Validate(model, melUlcer, requireInstitution: false);
            if (businessValidationErrors.Count > 0)
            {
                errorMessage = string.Join(Environment.NewLine, businessValidationErrors);
                return;
            }

            var payload = new
            {
                model.IsPublic,
                model.ImageType,
                model.ImageManipulation,
                model.DermoscopicType,
                model.AgeApprox,
                model.Sex,
                model.FotoType,
                PersonalHxMm = personalHxMm ? true : (bool?)null,
                FamilyHxMm = familyHxMm ? true : (bool?)null,
                model.AnatomSiteGeneral,
                model.AnatomSiteSpecial,
                model.ClinSizeLongDiamMm,
                model.Diagnosis,
                model.DiagnosisCategory,
                model.InjuryType,
                model.DiagnosisConfirmType,
                model.MelThickMm,
                model.MelMitoticIndex,
                MelUlcer = melUlcer ? true : (bool?)null,
                model.ClinicalNotes,
                ContributorId = ParseGuid(model.ContributorId),
            };

            var response = await Http.PutAsJsonAsync($"api/images/{Id}", payload);
            if (response.IsSuccessStatusCode)
            {
                Navigation.NavigateTo($"images/{Id}");
            }
            else
            {
                errorMessage = await ApiValidationMessageParser.BuildFriendlyErrorMessageAsync(response);
            }
        }
        catch (Exception ex)
        {
            errorMessage = $"Error al guardar cambios: {ex.Message}";
        }
        finally
        {
            submitting = false;
        }
    }

    private static bool CanEditImage(ClaimsPrincipal user, DermaImgDto image)
    {
        if (!(user.Identity?.IsAuthenticated ?? false))
        {
            return false;
        }

        if (user.IsInRole("Admin"))
        {
            return true;
        }

        var userIdText = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? user.FindFirst("sub")?.Value
            ?? user.FindFirst(ClaimTypes.Name)?.Value;

        return Guid.TryParse(userIdText, out var userId)
            && image.ContributorId.HasValue
            && image.ContributorId.Value == userId;
    }

    private static Guid ParseGuid(string? value)
    {
        return Guid.TryParse(value, out var id) ? id : Guid.Empty;
    }

    private static string FormatFileSize(long bytes)
    {
        if (bytes < 1024)
        {
            return $"{bytes} B";
        }

        var kb = bytes / 1024d;
        if (kb < 1024)
        {
            return $"{kb:F1} KB";
        }

        var mb = kb / 1024d;
        return $"{mb:F1} MB";
    }
}
