using System.ComponentModel.DataAnnotations;

namespace Presentation.Models;

public class UserRegistrationDto
{
    [Required]
    [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "Invalid email format.")]
    public string Email { get; set; } = null!;
    [Required]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters long.")]
    public string Password { get; set; } = null!;
    [Compare("Password", ErrorMessage = "Passwords do not match.")]
    public string ConfirmPassword { get; set; } = null!;

    [Required]
    [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters.")]
    [MinLength(2, ErrorMessage = "First name must be at least 2 characters long.")]
    [RegularExpression(@"^[a-zA-Z]+$", ErrorMessage = "First name can only contain letters.")]
    public string FirstName { get; set; } = null!;
    [Required]
    [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters.")]
    [MinLength(2, ErrorMessage = "Last name must be at least 2 characters long.")]
    public string LastName { get; set; } = null!;
}
