using System.ComponentModel.DataAnnotations;

namespace Application.DermaImage.DTOs;

public class UpdateProfileDto
{
    [Required, MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Phone]
    public string? PhoneNumber { get; set; }
}
