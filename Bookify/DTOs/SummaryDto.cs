using System;

namespace Bookify.DTOs
{
    public class SummaryDto
    {
        public int SummaryID { get; set; }
        public int? BookID { get; set; }
        public string? BookTitle { get; set; } // اسم الكتاب
        public int? ChapterID { get; set; }
        public string? ChapterTitle { get; set; } // اسم الشابتر
        public string? Content { get; set; } // محتوى الملخص
        public string? Source { get; set; }
        public DateTime CreateDate { get; set; }
    }
}