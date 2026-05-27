namespace Application.DermaImage.DTOs;

/// <summary>
/// A single cell in a cross-tabulation: RowKey × ColKey = Count.
/// </summary>
public class CrossTabBucketDto
{
    public string RowKey { get; set; } = string.Empty;
    public string RowLabel { get; set; } = string.Empty;
    public string ColKey { get; set; } = string.Empty;
    public string ColLabel { get; set; } = string.Empty;
    public int Count { get; set; }
}
