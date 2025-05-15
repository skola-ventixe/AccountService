﻿using System.ComponentModel.DataAnnotations;

namespace Presentation.Models;

public class UserSigninDto
{
    [Required]
    public string Email { get; set; } = null!;
    [Required]
    public string Password { get; set; } = null!;
}
