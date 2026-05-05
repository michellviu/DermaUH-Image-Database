namespace Web.DermaImage.Shared.Models;

public class ImageMetadataDefinitionDto
{
    public string Key { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string DataType { get; set; } = string.Empty;
    public string? AllowedValues { get; set; }
    public bool Required { get; set; }
    public string? Notes { get; set; }
}
