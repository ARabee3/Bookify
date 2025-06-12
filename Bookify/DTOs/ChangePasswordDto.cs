using System.ComponentModel.DataAnnotations;

namespace Bookify.DTOs;

public class ChangePasswordDto
{
    [Required]
    public string CurrentPassword { get; set; } // كلمة المرور الحالية

    [Required]
    [StringLength(100, ErrorMessage = "PASSWORD_MIN_LENGTH", MinimumLength = 6)] // نفس الـ Validation زي التسجيل
    public string NewPassword { get; set; } // كلمة المرور الجديدة

    [Required]
    [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")] // يتأكد إنها مطابقة للجديدة
    public string ConfirmNewPassword { get; set; } // تأكيد كلمة المرور الجديدة
}
