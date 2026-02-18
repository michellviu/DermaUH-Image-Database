using Domain.DermaImage.Entities.Enums;

namespace Domain.DermaImage.Entities;

public class DermaImgFilter
{
    public IReadOnlyCollection<ImageType>? ImageTypes { get; set; }
    public IReadOnlyCollection<DiagnosisCategory>? DiagnosisCategories { get; set; }
    public IReadOnlyCollection<Sex>? Sexes { get; set; }
    public IReadOnlyCollection<AnatomSiteGeneral>? AnatomSites { get; set; }
    public bool? IsPublic { get; set; }
    public string? DiagnosisContains { get; set; }
}
