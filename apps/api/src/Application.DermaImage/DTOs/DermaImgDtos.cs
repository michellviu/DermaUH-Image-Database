using Domain.DermaImage.Entities.Enums;
using System.ComponentModel.DataAnnotations;

namespace Application.DermaImage.DTOs;

// ── DermaImg DTOs ──────────────────────────────────────────────────────

public class CreateDermaImgDto
{
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
    public int? AgeApprox { get; set; }
    public Sex? Sex { get; set; }
    public PhotoType? FotoType { get; set; }
    public bool? PersonalHxMm { get; set; }
    public bool? FamilyHxMm { get; set; }

    // Lesion clinical
    public AnatomSiteGeneral? AnatomSiteGeneral { get; set; }
    public AnatomSiteSpecial? AnatomSiteSpecial { get; set; }
    public double? ClinSizeLongDiamMm { get; set; }

    // Diagnostic
    public string? Diagnosis { get; set; }
    public DiagnosisCategory? DiagnosisCategory { get; set; }
    public InjuryType? InjuryType { get; set; }
    public DiagnosisConfirmType? DiagnosisConfirmType { get; set; }

    // Histologic
    public double? MelThickMm { get; set; }
    public MelMitoticIndex? MelMitoticIndex { get; set; }
    public bool? MelUlcer { get; set; }

    public string? ClinicalNotes { get; set; }

    // Relationships
    public Guid ContributorId { get; set; }
    public Guid? InstitutionId { get; set; }
}

public class DermaImgResponseDto
{
    public Guid Id { get; set; }
    public string PublicId { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public bool IsPublic { get; set; }
    public ImageApprovalStatus ApprovalStatus { get; set; }
    public string? ReviewComment { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public Guid? ReviewedByUserId { get; set; }
    public string? ReviewedByFullName { get; set; }

    // Acquisition
    public ImageType? ImageType { get; set; }
    public ImageManipulation? ImageManipulation { get; set; }
    public DermoscopicType? DermoscopicType { get; set; }

    // Patient
    public int? AgeApprox { get; set; }
    public Sex? Sex { get; set; }
    public PhotoType? FotoType { get; set; }
    public bool? PersonalHxMm { get; set; }
    public bool? FamilyHxMm { get; set; }

    // Lesion clinical
    public AnatomSiteGeneral? AnatomSiteGeneral { get; set; }
    public AnatomSiteSpecial? AnatomSiteSpecial { get; set; }
    public double? ClinSizeLongDiamMm { get; set; }

    // Diagnostic
    public string? Diagnosis { get; set; }
    public DiagnosisCategory? DiagnosisCategory { get; set; }
    public InjuryType? InjuryType { get; set; }
    public DiagnosisConfirmType? DiagnosisConfirmType { get; set; }

    // Histologic
    public double? MelThickMm { get; set; }
    public MelMitoticIndex? MelMitoticIndex { get; set; }
    public bool? MelUlcer { get; set; }

    public string? ClinicalNotes { get; set; }
    public DateTime CreatedAt { get; set; }

    // Relationships
    public Guid ContributorId { get; set; }
    public string? ContributorFullName { get; set; }
    public Guid? InstitutionId { get; set; }
    public string? InstitutionName { get; set; }
}

public class ReviewImageUploadDto
{
    public bool Approve { get; set; }

    [MaxLength(500)]
    public string? Comment { get; set; }
}
