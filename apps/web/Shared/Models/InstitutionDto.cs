namespace Web.DermaImage.Shared.Models;

public class InstitutionDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Country { get; set; }
    public string? City { get; set; }
    public string? Address { get; set; }
    public string? Website { get; set; }
    public string? ContactEmail { get; set; }
    public string? LogoUrl { get; set; }
    public DateTime CreatedAt { get; set; }
}
