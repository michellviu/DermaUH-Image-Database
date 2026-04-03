using System.ComponentModel.DataAnnotations;

namespace Web.DermaImage.Shared.Models;

public class AssignRoleRequest
{
    [Required]
    public string Role { get; set; } = string.Empty;
}
