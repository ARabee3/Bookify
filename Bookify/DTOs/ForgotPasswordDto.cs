using System.ComponentModel.DataAnnotations;

namespace Bookify.DTOs;

public class ForgotPasswordDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }
}
