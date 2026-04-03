using Application.DermaImage.DTOs;
using Domain.DermaImage.Entities.Enums;

namespace Application.DermaImage.Validation;

public static class DermaImgValidationRules
{
    public static Dictionary<string, string[]> Validate(CreateDermaImgDto dto)
    {
        var errors = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

        AddIf(dto.DiagnosisCategory is not DiagnosisCategory.Malignant && dto.InjuryType.HasValue,
            nameof(dto.InjuryType),
            "El tipo de lesion solo puede establecerse cuando la categoria diagnostica es Malignant.",
            errors);

        AddIf(dto.DiagnosisCategory == DiagnosisCategory.Malignant && !dto.InjuryType.HasValue,
            nameof(dto.InjuryType),
            "Cuando la categoria diagnostica es Malignant, debe especificar el tipo de lesion.",
            errors);

        AddIf(dto.ImageType is not ImageType.Dermoscopic && dto.DermoscopicType.HasValue,
            nameof(dto.DermoscopicType),
            "DermoscopicType solo aplica cuando ImageType es Dermoscopic.",
            errors);

        var hasMelanomaHistology = dto.MelThickMm.HasValue || dto.MelMitoticIndex.HasValue || dto.MelUlcer.HasValue;
        AddIf(dto.InjuryType != InjuryType.Melanoma && hasMelanomaHistology,
            nameof(dto.InjuryType),
            "Los campos histologicos (MelThickMm, MelMitoticIndex, MelUlcer) solo aplican para InjuryType Melanoma.",
            errors);

        AddIf(dto.AgeApprox is < 0 or > 120,
            nameof(dto.AgeApprox),
            "La edad aproximada debe estar entre 0 y 120.",
            errors);

        AddIf(dto.ClinSizeLongDiamMm <= 0 && dto.ClinSizeLongDiamMm.HasValue,
            nameof(dto.ClinSizeLongDiamMm),
            "El diametro clinico debe ser mayor que 0.",
            errors);

        AddIf(dto.MelThickMm <= 0 && dto.MelThickMm.HasValue,
            nameof(dto.MelThickMm),
            "El espesor de melanoma debe ser mayor que 0.",
            errors);

        return errors.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToArray(), StringComparer.OrdinalIgnoreCase);
    }

    private static void AddIf(bool condition, string key, string message, Dictionary<string, List<string>> errors)
    {
        if (!condition)
        {
            return;
        }

        if (!errors.TryGetValue(key, out var messages))
        {
            messages = [];
            errors[key] = messages;
        }

        messages.Add(message);
    }
}
