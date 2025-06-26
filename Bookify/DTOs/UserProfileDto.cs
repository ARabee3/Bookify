using System;

namespace Bookify.DTOs
{
    public class UserProfileDto
    {
        public string UserId { get; set; }
        public string Username { get; set; } // من IdentityUser.UserName
        public string Email { get; set; }    // من IdentityUser.Email
        public DateTime? JoinedDate { get; set; } // سنحاول إيجاد بديل أو نتركها Nullable
        // public string? Bio { get; set; } // <<< تم الحذف

        public int Age { get; set; } // نفترض أن Age موجودة وغير Nullable في ApplicationUser
        public string? Specialization { get; set; }
        public string? Level { get; set; }
        public string? Interest { get; set; }
    }
}