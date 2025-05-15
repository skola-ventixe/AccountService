using System.ComponentModel.DataAnnotations;

namespace Presentation.Models;

public class UserRegistrationDto
{
    [Required]
    public string Email { get; set; } = null!;
    [Required]
    public string Password { get; set; } = null!;

    [Required]
    public string FirstName { get; set; } = null!;
    [Required]
    public string LastName { get; set; } = null!;
}
