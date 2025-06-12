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

        public DbSet<UserBookRating> UserBookRating { get; set; }


        public DbSet<UserDailyActivityLog> UserDailyActivityLogs { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            base.OnModelCreating(modelBuilder);


            // داخل OnModelCreating، بعد base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<UserBookRating>()
    .HasKey(ubr => new { ubr.UserID, ubr.BookID });



            // تعريف العلاقة مع ApplicationUser
            modelBuilder.Entity<UserBookRating>()
                .HasOne(ubr => ubr.User)
                .WithMany(u => u.BookRatings) // تأكد إن ApplicationUser فيه ICollection<UserBookRating> BookRatings
                .HasForeignKey(ubr => ubr.UserID)
                .OnDelete(DeleteBehavior.Restrict); // أو Cascade

            // تعريف العلاقة مع Book
            modelBuilder.Entity<UserBookRating>()
                .HasOne(ubr => ubr.Book)
                .WithMany(b => b.Ratings) // تأكد إن Book فيه ICollection<UserBookRating> Ratings
                .HasForeignKey(ubr => ubr.BookID)
                .OnDelete(DeleteBehavior.Cascade); // منطقي

            // داخل OnModelCreating، بعد base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<UserDailyActivityLog>()
                .HasIndex(ual => new { ual.UserID, ual.ActivityDate }) // نعمل Index على العمودين دول
                .IsUnique(); // ونخليه فريد (Unique)

            // تعريف العلاقة مع ApplicationUser (اختياري لو الـ Convention هيعرفها لوحده)
            modelBuilder.Entity<UserDailyActivityLog>()
                .HasOne(ual => ual.User)
                .WithMany() // ApplicationUser مش لازم يكون عنده Collection من الـ ActivityLogs مباشرة
                .HasForeignKey(ual => ual.UserID)
                .OnDelete(DeleteBehavior.Cascade); // لو المستخدم اتحذف، نحذف سجلات نشاطه


            // Book -> User (Uploader)
            modelBuilder.Entity<Book>()
                .HasOne(b => b.Uploader)
                .WithMany(u => u.UploadedBooks)
                .HasForeignKey(b => b.UploadedBy)
                .OnDelete(DeleteBehavior.NoAction); // غيّرناها لـ NoAction

            // Chapter -> Book
            modelBuilder.Entity<Chapter>()
                .HasOne(c => c.Book)
                .WithMany(b => b.Chapters)
                .HasForeignKey(c => c.BookID)
                .OnDelete(DeleteBehavior.Cascade);

            // Quiz -> Chapter & Book
            modelBuilder.Entity<Quiz>()
                .HasOne(q => q.Chapter)
                .WithMany(c => c.Quizzes)
                .HasForeignKey(q => q.ChapterID)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Quiz>()
                .HasOne(q => q.Book)
                .WithMany(b => b.Quizzes)
                .HasForeignKey(q => q.BookID)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.NoAction);

            // Question -> Quiz
            modelBuilder.Entity<Question>()
                .HasOne(q => q.Quiz)
                .WithMany(qu => qu.Questions)
                .HasForeignKey(q => q.QuizID)
                .OnDelete(DeleteBehavior.Cascade);

            // Recommendation -> User & Book
            modelBuilder.Entity<Recommendation>()
                .HasOne(r => r.User)
                .WithMany(u => u.Recommendations)
                .HasForeignKey(r => r.UserID)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Recommendation>()
                .HasOne(r => r.Book)
                .WithMany(b => b.Recommendations)
                .HasForeignKey(r => r.BookID)
                .OnDelete(DeleteBehavior.NoAction);

            // Summary -> User & Book
            modelBuilder.Entity<Summary>()
                .HasOne(s => s.User)
                .WithMany(u => u.Summaries)
                .HasForeignKey(s => s.UserID)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Summary>()
                .HasOne(s => s.Book)
                .WithMany(b => b.Summaries)
                .HasForeignKey(s => s.BookID)
                .OnDelete(DeleteBehavior.NoAction);

            // Progress -> User & Book
            modelBuilder.Entity<Progress>()
                .HasOne(p => p.User)
                .WithMany(u => u.Progresses)
                .HasForeignKey(p => p.UserID)
                .OnDelete(DeleteBehavior.NoAction); // غيّرناها لـ NoAction

            modelBuilder.Entity<Progress>()
                .HasOne(p => p.Book)
                .WithMany(b => b.Progresses)
                .HasForeignKey(p => p.BookID)
                .OnDelete(DeleteBehavior.NoAction); // غيّرناها لـ NoAction

            // UserBookRating -> User & Book
            modelBuilder.Entity<UserBookRating>()
                .HasOne(r => r.User)
                .WithMany(u => u.BookRatings)
                .HasForeignKey(r => r.UserID)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<UserBookRating>()
                .HasOne(r => r.Book)
                .WithMany(b => b.Ratings)
                .HasForeignKey(r => r.BookID)
                .OnDelete(DeleteBehavior.NoAction);

            // UserQuizResult -> User & Quiz
            modelBuilder.Entity<UserQuizResult>()
                .HasOne(r => r.User)
                .WithMany(u => u.QuizResults)
                .HasForeignKey(r => r.UserID)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<UserQuizResult>()
                .HasOne(r => r.Quiz)
                .WithMany(q => q.QuizResults)
                .HasForeignKey(r => r.QuizID)
                .OnDelete(DeleteBehavior.NoAction);


            // Question -> Answer (One-to-Many)
            modelBuilder.Entity<Answer>()
                .HasOne(a => a.Question) // Answer has one Question
                .WithMany(q => q.Answers) // Question has many Answers
                .HasForeignKey(a => a.QuestionID) // The link is QuestionID column in Answer table
                .OnDelete(DeleteBehavior.Cascade); // If a Question is deleted, delete its Answers too


            modelBuilder.Entity<Summary>()
        .HasOne(s => s.Chapter)      // Summary has one Chapter (or null)
        .WithMany()                // Chapter doesn't need a direct collection of Summaries (or we can add it later if needed)
        .HasForeignKey(s => s.ChapterID) // Foreign key is ChapterID
        .IsRequired(false)         // ChapterID is not required (Nullable)
        .OnDelete(DeleteBehavior.Cascade); // لو الشابتر اتحذف، نحذف ملخصاته (منطقي)

            // Summary -> Book (Many-to-One, Optional Book)
            // (العلاقة دي ممكن تكون ضمنية لو ربطنا بالـ Chapter، بس ممكن نعرفها احتياطي)
            modelBuilder.Entity<Summary>()
                .HasOne(s => s.Book)        // Summary can have one Book (or null)
                .WithMany(b => b.Summaries) // Book has many Summaries (دي موجودة في Book.cs)
                .HasForeignKey(s => s.BookID) // Foreign key is BookID
                .IsRequired(false)       // BookID is not required (Nullable)
                .OnDelete(DeleteBehavior.NoAction); // لو الكتاب اتحذف، منعملش حاجة للملخص (أو ممكن Cascade حسب الرغبة)

            // Summary -> User (Many-to-One, Optional User)
            modelBuilder.Entity<Summary>()
                .HasOne(s => s.User)        // Summary has one User (or null)
                .WithMany(u => u.Summaries) // User has many Summaries (دي موجودة في ApplicationUser.cs)
                .HasForeignKey(s => s.UserID) // Foreign key is UserID (string?)
                .IsRequired(false)       // UserID is not required (Nullable)
                .OnDelete(DeleteBehavior.NoAction); // لو المستخدم اتحذف، منعملش حاجة للملخص
        }
    }
}
