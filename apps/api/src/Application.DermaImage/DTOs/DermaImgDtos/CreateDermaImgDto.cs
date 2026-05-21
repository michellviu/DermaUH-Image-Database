using Domain.DermaImage.Entities.Enums;

namespace Application.DermaImage.DTOs;

/// <summary>
/// DTO para crear imágenes dermatológicas. Incluye propiedades internas del servidor.
/// </summary>
public class CreateDermaImgDto
{
    // These are set by the server, not by the client.
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }

    public bool IsPublic { get; set; }

    // Acquisition
    public ImageType? ImageType { get; set; }
    public ImageManipulation? ImageManipulation { get; set; }
    public DermoscopicType? DermoscopicType { get; set; }

    // Patient
    public string? PatientName { get; set; }
    public string? ClinicalHistoryNumber { get; set; }
    public int? AgeApprox { get; set; }
    public Sex? Sex { get; set; }
    public SkinColor? SkinColor { get; set; }
    public PhotoType? FotoType { get; set; }
    public string? PersonalHistory { get; set; }
    public bool? PersonalHxMm { get; set; }
    public bool? FamilyHxMm { get; set; }
    public bool? SunExposure { get; set; }

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
