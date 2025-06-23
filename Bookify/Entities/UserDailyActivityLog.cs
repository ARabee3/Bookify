using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bookify.Entities
{
    public class UserDailyActivityLog
    {
        [Key]
        public int ActivityLogID { get; set; }

        [Required]
        public string UserID { get; set; }
        public virtual ApplicationUser User { get; set; }

        [Required]
        [Column(TypeName = "date")]
        public DateTime ActivityDate { get; set; }
    }
}