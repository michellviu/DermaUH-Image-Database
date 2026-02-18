namespace Domain.DermaImage.Entities;

/// <summary>
/// Represents a medical institution that contributes images.
/// </summary>
public class Institution : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Country { get; set; }
    public string? City { get; set; }
    public string? Address { get; set; }
    public string? Website { get; set; }
    public string? ContactEmail { get; set; }
    public string? LogoUrl { get; set; }

    // Navigation
    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<DermaImg> Images { get; set; } = new List<DermaImg>();
}
