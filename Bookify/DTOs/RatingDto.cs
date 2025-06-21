// في مجلد Dtos
using System;

namespace Bookify.DTOs
{
    public class RatingDto
    {
        public int RatingID { get; set; }
        public string Username { get; set; } // اسم المستخدم اللي عمل التقييم (مش الـ ID)
        // public string UserProfileImageUrl { get; set; } // اختياري: صورة بروفايل المستخدم
        public int BookID { get; set; }
        public string BookTitle { get; set; } // اسم الكتاب (اختياري، ممكن يكون واضح من السياق)
        public float RatingValue { get; set; }
        public string? ReviewText { get; set; }
        public DateTime RatedAt { get; set; }
    }
}