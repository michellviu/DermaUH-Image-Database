namespace Application.DermaImage.DTOs;

/// <summary>
/// Count of melanoma images grouped by general anatomical site, for body map visualization.
/// </summary>
public class BodyMapEntryDto
{
    public string SiteKey { get; set; } = string.Empty;
    public string SiteLabel { get; set; } = string.Empty;
    public int MelanomaCount { get; set; }
    public int TotalCount { get; set; }
    public double MelanomaPercentage { get; set; }
}
