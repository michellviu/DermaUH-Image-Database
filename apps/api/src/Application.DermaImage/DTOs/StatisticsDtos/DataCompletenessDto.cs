namespace Application.DermaImage.DTOs;

/// <summary>
/// Tracks what percentage of images have a given metadata field populated.
/// </summary>
public class DataCompletenessDto
{
    public string FieldName { get; set; } = string.Empty;
    public string FieldLabel { get; set; } = string.Empty;
    public int FilledCount { get; set; }
    public int TotalCount { get; set; }
    public double Percentage { get; set; }
}
