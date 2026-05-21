using System.ComponentModel.DataAnnotations;

namespace Application.DermaImage.DTOs;

public class ForgotPasswordDto
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;
}
