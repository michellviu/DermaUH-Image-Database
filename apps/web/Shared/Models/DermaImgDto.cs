namespace Web.DermaImage.Shared.Models;

public class DermaImgDto
{
    public Guid Id { get; set; }
    public string PublicId { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string? CopyrightLicense { get; set; }
    public string? Attribution { get; set; }
    public bool IsPublic { get; set; }

    // Acquisition
    public string? ImageType { get; set; }
    public string? ImageManipulation { get; set; }
    public string? DermoscopicType { get; set; }
    public int? AcquisitionDay { get; set; }

    // Patient
    public int? AgeApprox { get; set; }
    public string? Sex { get; set; }
    public bool? PersonalHxMm { get; set; }
    public bool? FamilyHxMm { get; set; }

    // Lesion clinical
    public string? AnatomSiteGeneral { get; set; }
    public string? AnatomSiteSpecial { get; set; }
    public double? ClinSizeLongDiamMm { get; set; }

    // Diagnostic
    public string? Diagnosis { get; set; }
    public string? DiagnosisCategory { get; set; }
    public string? DiagnosisLevel2 { get; set; }
    public string? DiagnosisLevel3 { get; set; }
    public string? DiagnosisLevel4 { get; set; }
    public string? DiagnosisLevel5 { get; set; }
    public bool? ConcomitantBiopsy { get; set; }
    public string? DiagnosisConfirmType { get; set; }
    public bool? Melanocytic { get; set; }

    // Histologic
    public double? MelThickMm { get; set; }
    public string? MelMitoticIndex { get; set; }
    public bool? MelUlcer { get; set; }

    public string? ClinicalNotes { get; set; }
    public DateTime CreatedAt { get; set; }

    public Guid? ContributorId { get; set; }
    public Guid? InstitutionId { get; set; }
}
