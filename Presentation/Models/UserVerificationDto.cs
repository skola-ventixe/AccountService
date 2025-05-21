namespace Presentation.Models;

public class UserVerificationDto
{
    public string Email { get; set; } = null!;
    public string Code { get; set; } = null!;
}
