namespace Web.DermaImage.Shared.Models;

public class InstitutionDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Country { get; set; }
    public int ImageCount { get; set; }
}
