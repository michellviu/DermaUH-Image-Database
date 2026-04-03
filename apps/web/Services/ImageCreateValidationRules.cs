using Web.DermaImage.Shared.Models;

namespace Web.DermaImage.Services;

public static class ImageCreateValidationRules
{
    public static bool RequiresDermoscopicType(string? imageType)
        => string.Equals(imageType, "Dermoscopic", StringComparison.OrdinalIgnoreCase);

    public static bool AllowsInjuryType(string? diagnosisCategory)
        => string.Equals(diagnosisCategory, "Malignant", StringComparison.OrdinalIgnoreCase);

    public static bool AllowsMelanomaHistology(string? injuryType)
        => string.Equals(injuryType, "Melanoma", StringComparison.OrdinalIgnoreCase);

    public static void Normalize(CreateImageFormModel model, ref bool melUlcer)
    {
        if (!RequiresDermoscopicType(model.ImageType))
        {
            model.DermoscopicType = null;
        }

        if (!AllowsInjuryType(model.DiagnosisCategory))
        {
            model.InjuryType = null;
        }

        if (!AllowsMelanomaHistology(model.InjuryType))
        {
            model.MelThickMm = null;
            model.MelMitoticIndex = null;
            melUlcer = false;
        }
    }

    public static IReadOnlyList<string> Validate(CreateImageFormModel model, bool melUlcer, bool requireInstitution = true)
    {
        var errors = new List<string>();

        if (!RequiresDermoscopicType(model.ImageType) && !string.IsNullOrWhiteSpace(model.DermoscopicType))
        {
            errors.Add("El tipo dermatoscopico solo aplica cuando el tipo de imagen es Dermoscopic.");
        }

        if (!AllowsInjuryType(model.DiagnosisCategory) && !string.IsNullOrWhiteSpace(model.InjuryType))
        {
            errors.Add("El tipo de lesion solo puede seleccionarse cuando la categoria diagnostica es Maligno.");
        }

        if (AllowsInjuryType(model.DiagnosisCategory) && string.IsNullOrWhiteSpace(model.InjuryType))
        {
            errors.Add("Cuando la categoria diagnostica es Maligno, debe seleccionar el tipo de lesion.");
        }

        var hasMelanomaHistology = model.MelThickMm.HasValue || !string.IsNullOrWhiteSpace(model.MelMitoticIndex) || melUlcer;
        if (!AllowsMelanomaHistology(model.InjuryType) && hasMelanomaHistology)
        {
            errors.Add("Los campos histologicos solo aplican cuando el tipo de lesion es Melanoma.");
        }

        if (requireInstitution && string.IsNullOrWhiteSpace(model.InstitutionId))
        {
            errors.Add("Debe seleccionar una institucion.");
        }

        return errors;
    }
}
