namespace Web.DermaImage.Shared.Models;

public class CreateImageFormModel
{
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string ContentType { get; set; } = "image/jpeg";
    public long FileSize { get; set; }
    public bool IsPublic { get; set; }
    public string? ContributorId { get; set; }

    public string? ImageType { get; set; }
    public string? ImageManipulation { get; set; }
    public string? DermoscopicType { get; set; }

    public int? AgeApprox { get; set; }
    public string? Sex { get; set; }
    public string? FotoType { get; set; }

    public string? AnatomSiteGeneral { get; set; }
    public string? AnatomSiteSpecial { get; set; }
    public double? ClinSizeLongDiamMm { get; set; }

    public string? Diagnosis { get; set; }
    public string? DiagnosisCategory { get; set; }
    public string? InjuryType { get; set; }
    public string? DiagnosisConfirmType { get; set; }

    public double? MelThickMm { get; set; }
    public string? MelMitoticIndex { get; set; }

    public string? ClinicalNotes { get; set; }
    public string? InstitutionId { get; set; }
}
