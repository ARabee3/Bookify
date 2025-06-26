using Bookify.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Bookify.Contexts
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // --- DbSets الأساسية ---
        public DbSet<Book> Books { get; set; }
        public DbSet<Chapter> Chapters { get; set; }
        public DbSet<Summary> Summaries { get; set; }
        public DbSet<Quiz> Quizzes { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<Answer> Answers { get; set; }
        public DbSet<UserNote> UserNotes { get; set; }
        public DbSet<UserBookRating> UserBookRatings { get; set; }
        public DbSet<Recommendation> Recommendations { get; set; }
        public DbSet<UserQuizResult> UserQuizResults { get; set; }

        // --- DbSets للـ Progress والـ Activity Log ---
        public DbSet<Progress> Progresses { get; set; }
        public DbSet<UserDailyActivityLog> UserDailyActivityLogs { get; set; }

        // DbSets أخرى
        public DbSet<ReadingChallenge> ReadingChallenges { get; set; }
        public DbSet<Space> Spaces { get; set; }
        public DbSet<Participant> Participants { get; set; }

        public DbSet<UserLibraryBook> UserLibraryBooks { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            // داخل OnModelCreating
            modelBuilder.Entity<UserLibraryBook>(entity =>
            {
                // تعريف Composite Primary Key
                entity.HasKey(ulb => new { ulb.UserID, ulb.BookID });

                entity.HasOne(ulb => ulb.User)
                    .WithMany() // ApplicationUser مش لازم يكون عنده Collection من UserLibraryBooks
                    .HasForeignKey(ulb => ulb.UserID)
                    .IsRequired()
                    .OnDelete(DeleteBehavior.Cascade); // لو المستخدم اتحذف، نحذف كتب مكتبته

                entity.HasOne(ulb => ulb.Book)
                    .WithMany() // Book مش لازم يكون عنده Collection من UserLibraryBooks
                    .HasForeignKey(ulb => ulb.BookID)
                    .IsRequired()
                    .OnDelete(DeleteBehavior.Cascade); // لو الكتاب اتحذف (من النظام كله)، نحذفه من مكتبات المستخدمين
            });








            base.OnModelCreating(modelBuilder);

            // --- Book Entity Configuration ---
            modelBuilder.Entity<Book>(entity =>
            {
                entity.HasOne(b => b.Uploader)
                      .WithMany()
                      .HasForeignKey(b => b.UploadedBy)
                      .IsRequired(false)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // --- UserNote Entity Configuration ---
            modelBuilder.Entity<UserNote>(entity =>
            {
                entity.HasOne(un => un.User).WithMany().HasForeignKey(un => un.UserID).IsRequired().OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(un => un.Book).WithMany().HasForeignKey(un => un.BookID).IsRequired(false).OnDelete(DeleteBehavior.ClientSetNull);
                entity.HasOne(un => un.Chapter).WithMany().HasForeignKey(un => un.ChapterID).IsRequired(false).OnDelete(DeleteBehavior.ClientSetNull);
            });

            // --- UserBookRating Entity Configuration ---
            modelBuilder.Entity<UserBookRating>(entity =>
            {
                entity.HasKey(ubr => ubr.RatingID); // نفترض أن RatingID هو الـ Primary Key
                entity.HasIndex(ubr => new { ubr.UserID, ubr.BookID }).IsUnique();
                entity.HasOne(ubr => ubr.User).WithMany(u => u.BookRatings).HasForeignKey(ubr => ubr.UserID).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(ubr => ubr.Book).WithMany(b => b.Ratings).HasForeignKey(ubr => ubr.BookID).OnDelete(DeleteBehavior.Cascade);
            });

            // --- Chapter Entity Configuration ---
            modelBuilder.Entity<Chapter>(entity => {
                entity.HasOne(c => c.Book).WithMany(b => b.Chapters).HasForeignKey(c => c.BookID).OnDelete(DeleteBehavior.Cascade);
            });

            // --- Summary Entity Configuration ---
            modelBuilder.Entity<Summary>(entity => {
                entity.HasOne(s => s.Book).WithMany(b => b.Summaries).HasForeignKey(s => s.BookID).IsRequired(false).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(s => s.Chapter).WithMany().HasForeignKey(s => s.ChapterID).IsRequired(false).OnDelete(DeleteBehavior.Restrict); // Changed to Restrict to potentially avoid cycles with Book cascade
                entity.HasOne(s => s.User).WithMany().HasForeignKey(s => s.UserID).IsRequired(false).OnDelete(DeleteBehavior.SetNull);
            });

            // --- Quiz, Question, Answer Configurations ---
            modelBuilder.Entity<Quiz>(entity => {
                entity.HasOne(q => q.Book).WithMany(b => b.Quizzes).HasForeignKey(q => q.BookID).IsRequired(false).OnDelete(DeleteBehavior.ClientSetNull);
                entity.HasOne(q => q.Chapter).WithMany(c => c.Quizzes).HasForeignKey(q => q.ChapterID).IsRequired(false).OnDelete(DeleteBehavior.ClientSetNull);
            });

            modelBuilder.Entity<Question>(entity => {
                entity.HasOne(q => q.Quiz).WithMany(qz => qz.Questions).HasForeignKey(q => q.QuizID).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Answer>(entity => {
                entity.HasOne(a => a.Question).WithMany(q => q.Answers).HasForeignKey(a => a.QuestionID).OnDelete(DeleteBehavior.Cascade);
            });

            // --- UserQuizResult Configuration ---
            modelBuilder.Entity<UserQuizResult>(entity => {
                entity.HasOne(uqr => uqr.User).WithMany(u => u.QuizResults).HasForeignKey(uqr => uqr.UserID).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(uqr => uqr.Quiz).WithMany(q => q.QuizResults).HasForeignKey(uqr => uqr.QuizID).OnDelete(DeleteBehavior.Cascade);
            });

            // --- Recommendation Configuration ---
            modelBuilder.Entity<Recommendation>(entity => {
                entity.HasOne(r => r.User).WithMany(u => u.Recommendations).HasForeignKey(r => r.UserID).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(r => r.Book).WithMany(b => b.Recommendations).HasForeignKey(r => r.BookID).OnDelete(DeleteBehavior.Cascade);
            });


            // --- بداية Configurations للـ Progress والـ Activity Log ---
            modelBuilder.Entity<Progress>(entity =>
            {
                entity.HasIndex(p => new { p.UserID, p.BookID }).IsUnique();

                entity.HasOne(p => p.User)
                    .WithMany()
                    .HasForeignKey(p => p.UserID)
                    .IsRequired()
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(p => p.Book)
                    .WithMany(b => b.Progresses)
                    .HasForeignKey(p => p.BookID)
                    .IsRequired()
                    .OnDelete(DeleteBehavior.Cascade); // Changed back to Cascade, if a book is deleted, progress on it is irrelevant

                // --- تم حذف الجزء الخاص بـ LastReadChapter من هنا ---
            });

            modelBuilder.Entity<UserDailyActivityLog>(entity =>
            {
                entity.HasIndex(ual => new { ual.UserID, ual.ActivityDate }).IsUnique();
                entity.HasOne(ual => ual.User)
                    .WithMany()
                    .HasForeignKey(ual => ual.UserID)
                    .IsRequired()
                    .OnDelete(DeleteBehavior.Cascade);
            });
            // --- نهاية Configurations للـ Progress والـ Activity Log ---

            // Configurations للجداول الأخرى
            modelBuilder.Entity<ReadingChallenge>(entity =>
            {
                entity.HasIndex(rc => new { rc.UserID, rc.Year }).IsUnique();
                entity.HasOne(rc => rc.User).WithMany().HasForeignKey(rc => rc.UserID).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Space>(entity =>
            {
                entity.Property(s => s.Title).IsRequired().HasMaxLength(100);
                entity.HasOne(s => s.Host).WithMany().HasForeignKey(s => s.HostId).IsRequired().OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Participant>(entity =>
            {
                entity.HasKey(p => new { p.UserId, p.SpaceId });
                entity.HasOne(p => p.Space).WithMany(s => s.Participants).HasForeignKey(p => p.SpaceId).IsRequired().OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(p => new { p.SpaceId, p.AgoraUid }).IsUnique();
                entity.HasOne(p => p.User).WithMany().HasForeignKey(p => p.UserId).IsRequired().OnDelete(DeleteBehavior.Cascade);
                entity.Property(p => p.Role).HasConversion<string>().HasMaxLength(20);
            });
        }
    }
}