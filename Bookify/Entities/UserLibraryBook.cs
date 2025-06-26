using System;
using System.ComponentModel.DataAnnotations; // لو هنستخدم Attributes للـ Composite Key
using System.ComponentModel.DataAnnotations.Schema;

namespace Bookify.Entities
{
    // ممكن منستخدمش ID منفصل لو هنعمل Composite Key
    // [Table("UserLibraryBooks")] // اسم الجدول لو عايزه مختلف
    public class UserLibraryBook
    {
        // --- Composite Primary Key (يُعرف في AppDbContext.OnModelCreating) ---
        [Required]
        public string UserID { get; set; }
        public virtual ApplicationUser User { get; set; }

        [Required]
        public int BookID { get; set; }
        public virtual Book Book { get; set; }
        // -------------------------------------------------------------------

        public DateTime AddedAt { get; set; } = DateTime.UtcNow;
    }
}