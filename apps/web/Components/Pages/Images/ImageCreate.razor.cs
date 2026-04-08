using System.Globalization;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Web.DermaImage.Services;
using Web.DermaImage.Shared.Models;

namespace Web.DermaImage.Components.Pages.Images;

public partial class ImageCreate
{
    private const long MaxUploadSizeBytes = 20 * 1024 * 1024;

    private CreateImageFormModel model = new();
    private bool submitting;
    private string? errorMessage;
    private IBrowserFile? selectedFile;
    private string? _userInstitutionName;
    private string UserInstitutionDisplayValue => _userInstitutionName ?? "Sin institución";

    private bool personalHxMm;
    private bool familyHxMm;
    private bool melUlcer;

    private bool dermoscopicEnabled => ImageCreateValidationRules.RequiresDermoscopicType(model.ImageType);
    private bool injuryTypeEnabled => ImageCreateValidationRules.AllowsInjuryType(model.DiagnosisCategory);
    private bool melanomaHistologyEnabled => ImageCreateValidationRules.AllowsMelanomaHistology(model.InjuryType);

    protected override async Task OnInitializedAsync()
    {
        await LoadCurrentUserInstitutionAsync();
    }

    private async Task LoadCurrentUserInstitutionAsync()
    {
        var profile = await Auth.FetchProfileAsync();
        if (profile is null)
        {
            model.InstitutionId = null;
            _userInstitutionName = null;
            return;
        }

        model.InstitutionId = profile.InstitutionId?.ToString();
        _userInstitutionName = profile.InstitutionName;
    }

    private Task OnDependencySourceChanged()
    {
        ImageCreateValidationRules.Normalize(model, ref melUlcer);
        return Task.CompletedTask;
    }

    private void OnFileSelected(InputFileChangeEventArgs e)
    {
        selectedFile = e.File;

        model.FileName = selectedFile.Name;
        model.ContentType = string.IsNullOrWhiteSpace(selectedFile.ContentType)
            ? "application/octet-stream"
            : selectedFile.ContentType;
        model.FileSize = selectedFile.Size;

        var storageRoot = Configuration["ServerImageStoragePath"] ?? "ImageStorage";
        model.FilePath = Path.Combine(storageRoot, model.FileName);
    }

    private async Task HandleSubmit()
    {
        submitting = true;
        errorMessage = null;

        try
        {
            if (selectedFile is null)
            {
                errorMessage = "Debe seleccionar una imagen.";
                return;
            }

            if (string.IsNullOrWhiteSpace(model.InstitutionId))
            {
                errorMessage = "Debes pertenecer a una institución para subir imágenes.";
                return;
            }

            ImageCreateValidationRules.Normalize(model, ref melUlcer);

            var businessValidationErrors = ImageCreateValidationRules.Validate(model, melUlcer);
            if (businessValidationErrors.Count > 0)
            {
                errorMessage = string.Join(Environment.NewLine, businessValidationErrors);
                return;
            }

            using var formData = new MultipartFormDataContent();
            await using var fileStream = selectedFile.OpenReadStream(MaxUploadSizeBytes);
            using var fileContent = new StreamContent(fileStream);

            fileContent.Headers.ContentType = new MediaTypeHeaderValue(model.ContentType);
            formData.Add(fileContent, "file", selectedFile.Name);

            AddIfNotEmpty(formData, "FileName", model.FileName);
            AddIfNotEmpty(formData, "FilePath", model.FilePath);
            AddIfNotEmpty(formData, "ContentType", model.ContentType);
            AddIfNotEmpty(formData, "ImageType", model.ImageType);
            AddIfNotEmpty(formData, "ImageManipulation", model.ImageManipulation);
            AddIfNotEmpty(formData, "DermoscopicType", model.DermoscopicType);
            AddIfNotEmpty(formData, "Sex", model.Sex);
            AddIfNotEmpty(formData, "FotoType", model.FotoType);
            AddIfNotEmpty(formData, "AnatomSiteGeneral", model.AnatomSiteGeneral);
            AddIfNotEmpty(formData, "Diagnosis", model.Diagnosis);
            AddIfNotEmpty(formData, "DiagnosisCategory", model.DiagnosisCategory);
            AddIfNotEmpty(formData, "InjuryType", model.InjuryType);
            AddIfNotEmpty(formData, "DiagnosisConfirmType", model.DiagnosisConfirmType);
            AddIfNotEmpty(formData, "MelMitoticIndex", model.MelMitoticIndex);
            AddIfNotEmpty(formData, "ClinicalNotes", model.ClinicalNotes);
            AddIfNotEmpty(formData, "InstitutionId", model.InstitutionId);

            AddIfValue(formData, "AgeApprox", model.AgeApprox);
            AddIfValue(formData, "ClinSizeLongDiamMm", model.ClinSizeLongDiamMm);
            AddIfValue(formData, "MelThickMm", model.MelThickMm);

            AddNullableBool(formData, "PersonalHxMm", personalHxMm);
            AddNullableBool(formData, "FamilyHxMm", familyHxMm);
            AddNullableBool(formData, "MelUlcer", melUlcer);

            formData.Add(new StringContent(model.IsPublic.ToString()), "IsPublic");

            var response = await Http.PostAsync("api/images", formData);
            if (response.IsSuccessStatusCode)
            {
                Navigation.NavigateTo("images");
            }
            else
            {
                errorMessage = await ApiValidationMessageParser.BuildFriendlyErrorMessageAsync(response);
            }
        }
        catch (Exception ex)
        {
            errorMessage = $"Error: {ex.Message}";
        }
        finally
        {
            submitting = false;
        }
    }

    private static void AddIfNotEmpty(MultipartFormDataContent formData, string key, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            formData.Add(new StringContent(value), key);
        }
    }

    private static void AddIfValue<T>(MultipartFormDataContent formData, string key, T? value)
        where T : struct
    {
        if (value.HasValue)
        {
            var text = value.Value switch
            {
                double number => number.ToString(CultureInfo.InvariantCulture),
                float number => number.ToString(CultureInfo.InvariantCulture),
                decimal number => number.ToString(CultureInfo.InvariantCulture),
                _ => value.Value.ToString(),
            };

            if (!string.IsNullOrWhiteSpace(text))
            {
                formData.Add(new StringContent(text), key);
            }
        }
    }

    private static void AddNullableBool(MultipartFormDataContent formData, string key, bool value)
    {
        if (value)
        {
            formData.Add(new StringContent("true"), key);
        }
    }
}
