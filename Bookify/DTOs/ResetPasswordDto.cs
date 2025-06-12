using System.ComponentModel.DataAnnotations;

namespace Bookify.DTOs;

public class ResetPasswordDto
{
    [Required]
    public string Email { get; set; } // الإيميل من الـ URL أو الـ Body

    [Required]
    public string Token { get; set; } // الـ Token من الـ URL أو الـ Body

    [Required]
    [StringLength(100, ErrorMessage = "PASSWORD_MIN_LENGTH", MinimumLength = 6)] // نفس قواعد الباسورد
    public string NewPassword { get; set; }
}
