using System;
namespace Bookify.DTOs
{
    public class NoteDto
    {
        public int NoteID { get; set; }
        // public string UserID { get; set; } // ممكن نرجع اسم المستخدم بدل الـ ID
        public string Username { get; set; } // اسم المستخدم اللي عمل النوت
        public int? BookID { get; set; }
        public string? BookTitle { get; set; } // نجيبه من جدول Books
        public int? ChapterID { get; set; }
        public string? ChapterTitle { get; set; } // نجيبه من جدول Chapters
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastModifiedAt { get; set; }
    }
}