using Domain.DermaImage.Entities.Enums;

namespace Application.DermaImage.DTOs;

// ── DermaImg DTOs ──────────────────────────────────────────────────────

public class CreateDermaImgDto
{
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public CopyrightLicense? CopyrightLicense { get; set; }
    public string? Attribution { get; set; }
    public bool IsPublic { get; set; }

    // Acquisition
    public ImageType? ImageType { get; set; }
    public ImageManipulation? ImageManipulation { get; set; }
    public DermoscopicType? DermoscopicType { get; set; }
    public int? AcquisitionDay { get; set; }

    // Patient
    public int? AgeApprox { get; set; }
    public Sex? Sex { get; set; }
    public bool? PersonalHxMm { get; set; }
    public bool? FamilyHxMm { get; set; }

    // Lesion clinical
    public AnatomSiteGeneral? AnatomSiteGeneral { get; set; }
    public AnatomSiteSpecial? AnatomSiteSpecial { get; set; }
    public double? ClinSizeLongDiamMm { get; set; }

    // Diagnostic
    public string? Diagnosis { get; set; }
    public DiagnosisCategory? DiagnosisCategory { get; set; }
    public string? DiagnosisLevel2 { get; set; }
    public string? DiagnosisLevel3 { get; set; }
    public string? DiagnosisLevel4 { get; set; }
    public string? DiagnosisLevel5 { get; set; }
    public bool? ConcomitantBiopsy { get; set; }
    public DiagnosisConfirmType? DiagnosisConfirmType { get; set; }
    public bool? Melanocytic { get; set; }

    // Histologic
    public double? MelThickMm { get; set; }
    public MelMitoticIndex? MelMitoticIndex { get; set; }
    public bool? MelUlcer { get; set; }

    public string? ClinicalNotes { get; set; }

    // Relationships
    public Guid? ContributorId { get; set; }
    public Guid? InstitutionId { get; set; }
}
