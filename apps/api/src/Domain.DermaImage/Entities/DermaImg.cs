using Domain.DermaImage.Entities.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.DermaImage.Entities;

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

    /// <summary>Whether the image is visible to anonymous/public users.</summary>
    public bool IsPublic { get; set; } = false;

    /// <summary>Review lifecycle status for publication workflow.</summary>
    public ImageApprovalStatus ApprovalStatus { get; set; } = ImageApprovalStatus.Pending;

    /// <summary>Optional reviewer comment captured at decision time.</summary>
    public string? ReviewComment { get; set; }

    /// <summary>Timestamp when the review decision was made.</summary>
    public DateTime? ReviewedAt { get; set; }

    /// <summary>Reviewer user who made the decision.</summary>
    public Guid? ReviewedByUserId { get; set; }

    [ForeignKey("ReviewedByUserId")]
    public User? ReviewedByUser { get; set; }

    // ── Acquisition / Technologic ──────────────────────────────────────
    /// <summary>Dermatologic image type (dermoscopic, clinical, etc.).</summary>
    public ImageType? ImageType { get; set; }

    /// <summary>Fidelity of the image to the original capture.</summary>
    public ImageManipulation? ImageManipulation { get; set; }

    /// <summary>Modality of dermoscopic imaging (only when ImageType is Dermoscopic).</summary>
    public DermoscopicType? DermoscopicType { get; set; }

    // ── Patient Demographics ───────────────────────────────────────────
    /// <summary>Approximate age of the patient at time of capture (binned to 5-year intervals).</summary>
    public int? AgeApprox { get; set; }

    /// <summary>Biologic sex of the imaged person.</summary>
    public Sex? Sex { get; set; }

    /// <summary>Skin type.</summary>
    public PhotoType? FotoType { get; set; }

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

    /// <summary>Type of skin lesion (Melanoma, BasalCellCarcinoma, etc.).</summary>
    public InjuryType? InjuryType { get; set; }

    /// <summary>Method by which the diagnosis was confirmed.</summary>
    public DiagnosisConfirmType? DiagnosisConfirmType { get; set; }

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
    public Guid ContributorId { get; set; }
    
    [ForeignKey("ContributorId")]
    public User Contributor { get; set; } = null!;

    /// <summary>Institution that provided this image.</summary>
    public Guid? InstitutionId { get; set; }

    [ForeignKey("InstitutionId")]
    public Institution? Institution { get; set; }
}
