using System.ComponentModel.DataAnnotations;

namespace Application.DermaImage.DTOs;

public class GoogleLoginDto
{
    /// <summary>ID token returned by Google Identity / One Tap.</summary>
    [Required]
    public string IdToken { get; set; } = string.Empty;
}
