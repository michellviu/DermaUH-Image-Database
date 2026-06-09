using Domain.DermaImage.Entities.Enums;
using Microsoft.AspNetCore.Http;

namespace WebApi.DermaImage.DTOs;

/// <summary>
/// DTO para recibir formularios multipart/form-data. Excluye propiedades calculadas por el servidor.
/// </summary>
public class CreateDermaImgFormDto
{
    public IFormFile? File { get; set; }
    public bool IsPublic { get; set; }

    // Acquisition
    public ImageType? ImageType { get; set; }
    public ImageManipulation? ImageManipulation { get; set; }
    public DermoscopicType? DermoscopicType { get; set; }

    // Patient
    public int? AgeApprox { get; set; }
    public Sex? Sex { get; set; }
    public SkinColor? SkinColor { get; set; }
    public PhotoType? FotoType { get; set; }
    public string? PersonalHistory { get; set; }
    public bool? PersonalHxMm { get; set; }
    public bool? FamilyHxMm { get; set; }
    public bool? SunExposure { get; set; }
    public Provincia? Provincia { get; set; }

    // Lesion clinical
    public AnatomSiteGeneral? AnatomSiteGeneral { get; set; }
    public AnatomSiteSpecial? AnatomSiteSpecial { get; set; }
    public double? ClinSizeLongDiamMm { get; set; }

    // Diagnostic
    public string? Diagnosis { get; set; }
    public string? HistopathologicalDiagnosis { get; set; }
    public DiagnosisCategory? DiagnosisCategory { get; set; }
    public InjuryType? InjuryType { get; set; }
    public DiagnosisConfirmType? DiagnosisConfirmType { get; set; }

    // Histologic
    public double? MelThickMm { get; set; }
    public MelMitoticIndex? MelMitoticIndex { get; set; }
    public bool? MelUlcer { get; set; }

    public string? ClinicalNotes { get; set; }
    public string? DermoscopicComments { get; set; }
    public bool? InformedConsent { get; set; }
    public DateTime? InformedConsentDate { get; set; }
    public string? InformedConsentText { get; set; }

    // Relationships
    public Guid ContributorId { get; set; }
    public string? InstitutionName { get; set; }
    public string? InstitutionDescription { get; set; }
    public string? InstitutionCountry { get; set; }
}
