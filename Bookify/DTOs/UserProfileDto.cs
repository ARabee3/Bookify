using System;

namespace Bookify.DTOs
{
    public class UserProfileDto
    {
        public string UserId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public DateTime? JoinedDate { get; set; }
        //public string? Bio { get; set; } // لو كنت ضفتها
        public int Age { get; set; }
        public string? Specialization { get; set; }
        public string? Level { get; set; }
        public string? Interest { get; set; }

        // --- تأكد من وجود هذه الخاصية بالظبط كده ---
        public string? ProfilePictureFullUrl { get; set; }
        // ------------------------------------------
    }
}