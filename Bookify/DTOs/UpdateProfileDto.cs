using System.ComponentModel.DataAnnotations;

namespace Bookify.DTOs
{
    public class UpdateProfileDto
    {
        public string? Username { get; set; } // <<< تم إضافة هذه

        public int? Age { get; set; }
        public string? Specialization { get; set; }
        public string? Level { get; set; }
        public string? Interest { get; set; }
        // Bio تم حذفه بناءً على طلبك السابق
    }
}