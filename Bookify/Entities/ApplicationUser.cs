using Microsoft.AspNetCore.Identity; // مهمة جداً
using System.Collections.Generic; // عشان ICollection

namespace Bookify.Entities
{
    // هنرث من IdentityUser عشان ناخد كل المميزات الجاهزة
    public class ApplicationUser : IdentityUser
    {
        // --- الخصائص الإضافية اللي جبناها من User.cs القديم ---
        public int Age { get; set; }
        public string? Specialization { get; set; }
        public string? Level { get; set; }
        public string? Interest { get; set; }

        public int CurrentReadingStreak { get; set; } = 0; // الـ Streak الحالي (عدد الأيام المتتالية)
        public int LongestReadingStreak { get; set; } = 0; // أطول Streak حققه المستخدم
        public DateTime? LastStreakActivityDate { get; set; } // آخر تاريخ تم فيه نشاط وحساب الـ Streak 
        // ملاحظة: Name مش محتاجينها لو هنستخدم UserName اللي جاي من IdentityUser
        // ملاحظة: Email موجودة جاهزة في IdentityUser
        // ملاحظة: PasswordHash موجودة جاهزة في IdentityUser (و Identity بيحسبها)

        // --- الـ Collections اللي جبناها من User.cs القديم ---
        // (بنحط قبلها virtual عشان الـ Lazy Loading يشتغل صح مع EF Core)
        public virtual ICollection<Book> UploadedBooks { get; set; } = new List<Book>();
        public virtual ICollection<Recommendation> Recommendations { get; set; } = new List<Recommendation>();
        public virtual ICollection<Progress> Progresses { get; set; } = new List<Progress>();
        public virtual ICollection<Summary> Summaries { get; set; } = new List<Summary>();
        public virtual ICollection<UserQuizResult> QuizResults { get; set; } = new List<UserQuizResult>();
        public virtual ICollection<UserBookRating> BookRatings { get; set; } = new List<UserBookRating>();

        // ممكن نضيف هنا علاقة الـ Favorites لو عملنا Entity ليها بعدين
        // public virtual ICollection<UserFavoriteBook> FavoriteBooks { get; set; } = new List<UserFavoriteBook>();
    }
}