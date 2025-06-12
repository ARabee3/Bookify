using System.ComponentModel.DataAnnotations;

namespace Bookify.DTOs
{
    public class RegisterDto
    {
        [Required(ErrorMessage = "Username is required")] // مثال: نضيف رسالة خطأ
        public string Username { get; set; } // هنستخدم ده كـ UserName في Identity

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; } // للتأكيد

        // --- ممكن نضيف هنا بيانات البروفايل الإضافية لو عايزين نسجلها مرة واحدة ---
        public int? Age { get; set; } // خليها Nullable لو مش إجبارية
        public string? Specialization { get; set; }
        public string? Level { get; set; }
        public string? Interest { get; set; }
    }
}