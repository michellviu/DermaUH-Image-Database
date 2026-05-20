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

    /// <summary>Legacy publication status retained for compatibility.</summary>
    public ImageApprovalStatus ApprovalStatus { get; set; } = ImageApprovalStatus.Approved;

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
    /// <summary>Patient name as recorded in the clinical record.</summary>
    public string? PatientName { get; set; }

    /// <summary>Clinical history record identifier.</summary>
    public string? ClinicalHistoryNumber { get; set; }

    /// <summary>Approximate age of the patient at time of capture (binned to 5-year intervals).</summary>
    public int? AgeApprox { get; set; }

    /// <summary>Biologic sex of the imaged person.</summary>
    public Sex? Sex { get; set; }

    /// <summary>Skin color classification.</summary>
    public SkinColor? SkinColor { get; set; }

    /// <summary>Skin type.</summary>
    public PhotoType? FotoType { get; set; }

    /// <summary>Personal background notes provided by the contributor.</summary>
    public string? PersonalHistory { get; set; }

    /// <summary>Personal history of melanoma.</summary>
    public bool? PersonalHxMm { get; set; }

    /// <summary>Family history of melanoma in first-degree relative.</summary>
    public bool? FamilyHxMm { get; set; }

    /// <summary>Reported sun exposure.</summary>
    public bool? SunExposure { get; set; }

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

    /// <summary>Histopathological diagnosis, if available.</summary>
    public string? HistopathologicalDiagnosis { get; set; }

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

    /// <summary>Dermoscopic observations recorded by the specialist.</summary>
    public string? DermoscopicComments { get; set; }

    /// <summary>Indicates if informed consent was provided.</summary>
    public bool? InformedConsent { get; set; }

    /// <summary>Date when the informed consent was signed.</summary>
    public DateTime? InformedConsentDate { get; set; }

    /// <summary>Informed consent document text or reference.</summary>
    public string? InformedConsentText { get; set; }

    // ── Relationships ──────────────────────────────────────────────────
    /// <summary>External contributor identifier (not linked to local users).</summary>
    public Guid ContributorId { get; set; }

    /// <summary>Name of the contributing institution.</summary>
    public string? InstitutionName { get; set; }

    /// <summary>Description of the contributing institution.</summary>
    public string? InstitutionDescription { get; set; }

    /// <summary>Country of the contributing institution.</summary>
    public string? InstitutionCountry { get; set; }
}
