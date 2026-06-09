using System.ComponentModel.DataAnnotations;

namespace Application.DermaImage.DTOs;

public class RegisterDto
{
    [Required, MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(8)]
    public string Password { get; set; } = string.Empty;

    [Range(typeof(bool), "true", "true", ErrorMessage = "Debe aceptar los términos y condiciones.")]
    public bool AcceptTerms { get; set; }
}
