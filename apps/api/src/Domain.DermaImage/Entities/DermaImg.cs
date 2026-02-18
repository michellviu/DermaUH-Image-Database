using Domain.DermaImage.Entities.Enums;

namespace Domain.DermaImage.Entities;

/// <summary>
/// Represents a dermatological image record in the database.
/// </summary>
public class DermaImg : BaseEntity
{
    // ── Identification & Housekeeping ──────────────────────────────────
    /// <summary>Unique public identifier for the image (e.g., DERM_0000001).</summary>
    public string PublicId { get; set; } = string.Empty;

    /// <summary>Original filename uploaded by the contributor.</summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>Stored file path or URL to the image.</summary>
    public string FilePath { get; set; } = string.Empty;

    /// <summary>MIME type of the image file (e.g., image/jpeg).</summary>
    public string ContentType { get; set; } = string.Empty;

    /// <summary>File size in bytes.</summary>
    public long FileSize { get; set; }

    /// <summary>Creative Commons license subtype.</summary>
    public CopyrightLicense? CopyrightLicense { get; set; }

    /// <summary>Attribution text for Creative Commons compliance.</summary>
    public string? Attribution { get; set; }

    /// <summary>Whether the image is visible to anonymous/public users.</summary>
    public bool IsPublic { get; set; } = false;

    // ── Acquisition / Technologic ──────────────────────────────────────
    /// <summary>Dermatologic image type (dermoscopic, clinical, etc.).</summary>
    public ImageType? ImageType { get; set; }

    /// <summary>Fidelity of the image to the original capture.</summary>
    public ImageManipulation? ImageManipulation { get; set; }

    /// <summary>Modality of dermoscopic imaging (only when ImageType is Dermoscopic).</summary>
    public DermoscopicType? DermoscopicType { get; set; }

    /// <summary>Relative date of image capture compared to other images of the same patient.</summary>
    public int? AcquisitionDay { get; set; }

    // ── Patient Demographics ───────────────────────────────────────────
    /// <summary>Approximate age of the patient at time of capture (binned to 5-year intervals).</summary>
    public int? AgeApprox { get; set; }

    /// <summary>Biologic sex of the imaged person.</summary>
    public Sex? Sex { get; set; }

    /// <summary>Personal history of melanoma.</summary>
    public bool? PersonalHxMm { get; set; }

    /// <summary>Family history of melanoma in first-degree relative.</summary>
    public bool? FamilyHxMm { get; set; }

    // ── Lesion Clinical ────────────────────────────────────────────────
    /// <summary>General anatomic location of the lesion.</summary>
    public AnatomSiteGeneral? AnatomSiteGeneral { get; set; }

    /// <summary>Special anatomic site (acral, nail, etc.).</summary>
    public AnatomSiteSpecial? AnatomSiteSpecial { get; set; }

    /// <summary>Longest diameter of the lesion segmentation boundary in mm.</summary>
    public double? ClinSizeLongDiamMm { get; set; }

    // ── Lesion Diagnostic ──────────────────────────────────────────────
    /// <summary>Textual diagnosis provided by the contributor.</summary>
    public string? Diagnosis { get; set; }

    /// <summary>Malignancy super-category (benign/indeterminate/malignant).</summary>
    public DiagnosisCategory? DiagnosisCategory { get; set; }

    /// <summary>Second-level diagnosis classification.</summary>
    public string? DiagnosisLevel2 { get; set; }

    /// <summary>Third-level diagnosis term.</summary>
    public string? DiagnosisLevel3 { get; set; }

    /// <summary>Fourth-level diagnosis subterm.</summary>
    public string? DiagnosisLevel4 { get; set; }

    /// <summary>Fifth-level diagnosis subterm.</summary>
    public string? DiagnosisLevel5 { get; set; }

    /// <summary>Whether the image was captured at/just prior to biopsy.</summary>
    public bool? ConcomitantBiopsy { get; set; }

    /// <summary>Method by which the diagnosis was confirmed.</summary>
    public DiagnosisConfirmType? DiagnosisConfirmType { get; set; }

    /// <summary>Whether the lesion is melanocytic.</summary>
    public bool? Melanocytic { get; set; }

    // ── Lesion Histologic ──────────────────────────────────────────────
    /// <summary>Melanoma thickness (Breslow depth) in mm.</summary>
    public double? MelThickMm { get; set; }

    /// <summary>Mitotic index when diagnosis is melanoma.</summary>
    public MelMitoticIndex? MelMitoticIndex { get; set; }

    /// <summary>Presence of histopathologic ulceration.</summary>
    public bool? MelUlcer { get; set; }

    // ── Notes ──────────────────────────────────────────────────────────
    /// <summary>Free-text clinical notes or observations.</summary>
    public string? ClinicalNotes { get; set; }

    // ── Relationships ──────────────────────────────────────────────────
    /// <summary>User who contributed this image.</summary>
    public Guid? ContributorId { get; set; }
    public User? Contributor { get; set; }

    /// <summary>Institution that provided this image.</summary>
    public Guid? InstitutionId { get; set; }
    public Institution? Institution { get; set; }
}
