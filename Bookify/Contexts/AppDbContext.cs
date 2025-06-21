using Microsoft.EntityFrameworkCore;
using Bookify.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace Bookify.Contexts
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Book> Books { get; set; }
        public DbSet<Recommendation> Recommendations { get; set; }
        public DbSet<Summary> Summaries { get; set; }
        public DbSet<Progress> Progresses { get; set; }
        public DbSet<UserQuizResult> UserQuizResults { get; set; }
        public DbSet<Quiz> Quizzes { get; set; }
        public DbSet<Chapter> Chapters { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<Answer> Answers { get; set; } // <<< تأكد من وجود DbSet للـ Answer

        public DbSet<ReadingChallenge> ReadingChallenges { get; set; }
        public DbSet<UserBookRating> UserBookRatings { get; set; } // <<< تم تغيير الاسم هنا للجمع
        public DbSet<Space> Spaces { get; set; }
        public DbSet<Participant> Participants { get; set; }
        public DbSet<UserDailyActivityLog> UserDailyActivityLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ... (كل تعريفات العلاقات الأخرى كما هي) ...

            // --- تعديل في تعريف UserBookRating ---
            // تعريف Primary Key مركب (مستخدم + كتاب = تقييم واحد)
            modelBuilder.Entity<UserBookRating>()
                .HasKey(ubr => new { ubr.UserID, ubr.BookID }); // هذا يزيل الحاجة لـ RatingID كـ PK و Identity

            // تعريف العلاقة مع ApplicationUser
            modelBuilder.Entity<UserBookRating>()
                .HasOne(ubr => ubr.User)
                .WithMany(u => u.BookRatings) // تأكد أن ApplicationUser.cs فيه ICollection<UserBookRating> BookRatings { get; set; }
                .HasForeignKey(ubr => ubr.UserID)
                .OnDelete(DeleteBehavior.Restrict); // جيد

            // تعريف العلاقة مع Book
            modelBuilder.Entity<UserBookRating>()
                .HasOne(ubr => ubr.Book)
                .WithMany(b => b.Ratings) // Book.cs فيه ICollection<UserBookRating> Ratings { get; set; }
                .HasForeignKey(ubr => ubr.BookID)
                .OnDelete(DeleteBehavior.Cascade); // منطقي
            // --- نهاية تعديل UserBookRating ---

            // ... (تكملة باقي تعريفات العلاقات الأخرى كما هي) ...

            // تأكد من وجود تعريفات العلاقات لـ Summary, Answer, ReadingChallenge, UserDailyActivityLog, Space, Participant
            // (الكود اللي بعته كان فيه تكرار لتعريفات Summary، شيلت التكرار واحتفظت بالنسخة الأحدث)

            // Summary -> Chapter
            modelBuilder.Entity<Summary>()
                .HasOne(s => s.Chapter)
                .WithMany() // Chapter لا يحتاج لـ Collection of Summaries مباشرة
                .HasForeignKey(s => s.ChapterID)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Cascade);

            // Answer -> Question
            modelBuilder.Entity<Answer>()
                .HasOne(a => a.Question)
                .WithMany(q => q.Answers) // Question.cs يجب أن يحتوي على ICollection<Answer> Answers
                .HasForeignKey(a => a.QuestionID)
                .OnDelete(DeleteBehavior.Cascade);

            // ReadingChallenge (كما هو)
            modelBuilder.Entity<ReadingChallenge>(entity =>
            {
                entity.HasIndex(rc => new { rc.UserID, rc.Year }).IsUnique();
                entity.HasOne(rc => rc.User).WithMany().HasForeignKey(rc => rc.UserID).OnDelete(DeleteBehavior.Cascade);
            });

            // UserDailyActivityLog (كما هو)
            modelBuilder.Entity<UserDailyActivityLog>(entity => {
                entity.HasIndex(ual => new { ual.UserID, ual.ActivityDate }).IsUnique();
                entity.HasOne(ual => ual.User).WithMany().HasForeignKey(ual => ual.UserID).OnDelete(DeleteBehavior.Cascade);
            });

            // Space and Participant (كما هي)
            modelBuilder.Entity<Space>(entity =>
            {
                entity.HasKey(s => s.Id);
                entity.Property(s => s.Title).IsRequired().HasMaxLength(100);
                entity.HasOne(s => s.Host).WithMany().HasForeignKey(s => s.HostId).IsRequired().OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Participant>(entity =>
            {
                // entity.HasKey(p => new { p.UserId, p.SpaceId }); // Composite Key (اختياري)
                entity.HasOne(p => p.Space).WithMany(s => s.Participants).HasForeignKey(p => p.SpaceId).IsRequired().OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(p => new { p.SpaceId, p.AgoraUid }).IsUnique();
                entity.HasOne(p => p.User).WithMany().HasForeignKey(p => p.UserId).IsRequired().OnDelete(DeleteBehavior.Cascade);
                entity.Property(p => p.Role).HasConversion<string>().HasMaxLength(20);
            });
        }
    }
}