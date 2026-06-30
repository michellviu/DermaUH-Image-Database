using Application.DermaImage.DTOs;
using Application.DermaImage.Validation;
using Domain.DermaImage.Entities.Enums;
using FluentAssertions;

namespace Tests.Unit.DermaImage.Validation;

/// <summary>
/// Pruebas unitarias para las reglas de validación de negocio del dominio de imágenes dermatoscópicas.
/// Verifican que <see cref="DermaImgValidationRules.Validate"/> detecte correctamente
/// las violaciones de cada regla independientemente de la base de datos.
/// </summary>
public class DermaImgValidationRulesTests
{
    // ── Helpers ─────────────────────────────────────────────────────────────

    /// <summary>Construye un DTO mínimamente válido para usar como base en cada prueba.</summary>
    private static CreateDermaImgDto ValidDto() => new()
    {
        FileName    = "test.jpg",
        FilePath    = "/uploads/test.jpg",
        ContentType = "image/jpeg",
        FileSize    = 102_400,
        ContributorId = Guid.NewGuid(),
        AgeApprox   = 35,
        Sex         = Sex.Male,
        AnatomSiteGeneral = AnatomSiteGeneral.HeadNeck,
        Diagnosis   = "Nevo melanocítico",
        SunExposure = false
    };

    // ── Campo: AgeApprox ────────────────────────────────────────────────────

    [Fact(DisplayName = "AgeApprox ausente genera error de validación")]
    public void Validate_WhenAgeApproxIsNull_ReturnsError()
    {
        var dto = ValidDto();
        dto.AgeApprox = null;

        var errors = DermaImgValidationRules.Validate(dto);

        errors.Should().ContainKey(nameof(dto.AgeApprox));
    }

    [Theory(DisplayName = "AgeApprox fuera de rango [0,120] genera error")]
    [InlineData(-1)]
    [InlineData(121)]
    [InlineData(200)]
    public void Validate_WhenAgeApproxOutOfRange_ReturnsError(int age)
    {
        var dto = ValidDto();
        dto.AgeApprox = age;

        var errors = DermaImgValidationRules.Validate(dto);

        errors.Should().ContainKey(nameof(dto.AgeApprox));
    }

    [Theory(DisplayName = "AgeApprox dentro de rango [0,120] no genera error")]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(60)]
    [InlineData(120)]
    public void Validate_WhenAgeApproxWithinRange_NoError(int age)
    {
        var dto = ValidDto();
        dto.AgeApprox = age;

        var errors = DermaImgValidationRules.Validate(dto);

        errors.Should().NotContainKey(nameof(dto.AgeApprox));
    }

    // ── Campo: Sex ──────────────────────────────────────────────────────────

    [Fact(DisplayName = "Sex ausente genera error de validación")]
    public void Validate_WhenSexIsNull_ReturnsError()
    {
        var dto = ValidDto();
        dto.Sex = null;

        var errors = DermaImgValidationRules.Validate(dto);

        errors.Should().ContainKey(nameof(dto.Sex));
    }

    // ── Campo: AnatomSiteGeneral ─────────────────────────────────────────────

    [Fact(DisplayName = "AnatomSiteGeneral ausente genera error de validación")]
    public void Validate_WhenAnatomSiteGeneralIsNull_ReturnsError()
    {
        var dto = ValidDto();
        dto.AnatomSiteGeneral = null;

        var errors = DermaImgValidationRules.Validate(dto);

        errors.Should().ContainKey(nameof(dto.AnatomSiteGeneral));
    }

    // ── Campo: Diagnosis ────────────────────────────────────────────────────

    [Theory(DisplayName = "Diagnosis vacía o nula genera error de validación")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WhenDiagnosisIsNullOrWhitespace_ReturnsError(string? diagnosis)
    {
        var dto = ValidDto();
        dto.Diagnosis = diagnosis;

        var errors = DermaImgValidationRules.Validate(dto);

        errors.Should().ContainKey(nameof(dto.Diagnosis));
    }

    // ── Campo: SunExposure ──────────────────────────────────────────────────

    [Fact(DisplayName = "SunExposure ausente genera error de validación")]
    public void Validate_WhenSunExposureIsNull_ReturnsError()
    {
        var dto = ValidDto();
        dto.SunExposure = null;

        var errors = DermaImgValidationRules.Validate(dto);

        errors.Should().ContainKey(nameof(dto.SunExposure));
    }

    // ── Regla cruzada: InjuryType sólo para Malignant ───────────────────────

    [Fact(DisplayName = "InjuryType sin DiagnosisCategory=Malignant genera error")]
    public void Validate_WhenInjuryTypeSetWithoutMalignantCategory_ReturnsError()
    {
        var dto = ValidDto();
        dto.DiagnosisCategory = DiagnosisCategory.Benign;
        dto.InjuryType = InjuryType.Melanoma;

        var errors = DermaImgValidationRules.Validate(dto);

        errors.Should().ContainKey(nameof(dto.InjuryType));
    }

    [Fact(DisplayName = "InjuryType con DiagnosisCategory=Malignant no genera error")]
    public void Validate_WhenInjuryTypeSetWithMalignantCategory_NoError()
    {
        var dto = ValidDto();
        dto.DiagnosisCategory = DiagnosisCategory.Malignant;
        dto.InjuryType = InjuryType.Melanoma;

        var errors = DermaImgValidationRules.Validate(dto);

        errors.Should().NotContainKey(nameof(dto.InjuryType));
    }

    // ── Regla cruzada: DermoscopicType sólo para imágenes dermoscópicas ─────

    [Fact(DisplayName = "DermoscopicType sin ImageType=Dermoscopic genera error")]
    public void Validate_WhenDermoscopicTypeSetWithoutDermoscopicImageType_ReturnsError()
    {
        var dto = ValidDto();
        dto.ImageType = ImageType.ClinicalOverview;
        dto.DermoscopicType = DermoscopicType.ContactPolarized;

        var errors = DermaImgValidationRules.Validate(dto);

        errors.Should().ContainKey(nameof(dto.DermoscopicType));
    }

    [Fact(DisplayName = "DermoscopicType con ImageType=Dermoscopic no genera error")]
    public void Validate_WhenDermoscopicTypeSetWithDermoscopicImageType_NoError()
    {
        var dto = ValidDto();
        dto.ImageType = ImageType.Dermoscopic;
        dto.DermoscopicType = DermoscopicType.ContactPolarized;

        var errors = DermaImgValidationRules.Validate(dto);

        errors.Should().NotContainKey(nameof(dto.DermoscopicType));
    }

    // ── Regla cruzada: campos histológicos sólo para Melanoma ───────────────

    [Fact(DisplayName = "MelThickMm sin InjuryType=Melanoma genera error")]
    public void Validate_WhenMelThickMmSetWithoutMelanomaInjuryType_ReturnsError()
    {
        var dto = ValidDto();
        dto.DiagnosisCategory = DiagnosisCategory.Malignant;
        dto.InjuryType = InjuryType.BasalCellCarcinoma;
        dto.MelThickMm = 1.5;

        var errors = DermaImgValidationRules.Validate(dto);

        errors.Should().ContainKey(nameof(dto.InjuryType));
    }

    // ── Campo: ClinSizeLongDiamMm ───────────────────────────────────────────

    [Theory(DisplayName = "ClinSizeLongDiamMm <= 0 genera error")]
    [InlineData(0.0)]
    [InlineData(-5.0)]
    public void Validate_WhenClinSizeLongDiamMmIsNotPositive_ReturnsError(double value)
    {
        var dto = ValidDto();
        dto.ClinSizeLongDiamMm = value;

        var errors = DermaImgValidationRules.Validate(dto);

        errors.Should().ContainKey(nameof(dto.ClinSizeLongDiamMm));
    }

    [Fact(DisplayName = "ClinSizeLongDiamMm positivo no genera error")]
    public void Validate_WhenClinSizeLongDiamMmIsPositive_NoError()
    {
        var dto = ValidDto();
        dto.ClinSizeLongDiamMm = 12.5;

        var errors = DermaImgValidationRules.Validate(dto);

        errors.Should().NotContainKey(nameof(dto.ClinSizeLongDiamMm));
    }

    // ── DTO completamente válido ─────────────────────────────────────────────

    [Fact(DisplayName = "DTO completamente válido no genera ningún error")]
    public void Validate_WhenDtoIsFullyValid_ReturnsEmptyErrors()
    {
        var errors = DermaImgValidationRules.Validate(ValidDto());

        errors.Should().BeEmpty();
    }

    // ── Múltiples errores simultáneos ────────────────────────────────────────

    [Fact(DisplayName = "DTO vacío acumula múltiples errores de validación")]
    public void Validate_WhenDtoIsEmpty_ReturnsMultipleErrors()
    {
        var dto = new CreateDermaImgDto();   // todos los campos opcionales/nulos

        var errors = DermaImgValidationRules.Validate(dto);

        // Al menos Age, Sex, AnatomSiteGeneral, Diagnosis, SunExposure deben fallar
        errors.Count.Should().BeGreaterThanOrEqualTo(5);
    }
}
