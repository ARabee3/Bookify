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
        public DbSet<Recommendation> Recommendations { get; set; } // كان موجود
        public DbSet<UserQuizResult> UserQuizResults { get; set; } // كان موجود

        // --- DbSets الجديدة للـ Progress والـ Activity Log ---
        public DbSet<Progress> Progresses { get; set; }
        public DbSet<UserDailyActivityLog> UserDailyActivityLogs { get; set; }
        // ----------------------------------------------------

        // DbSets أخرى (كانت موجودة في الكود اللي بعته)
        public DbSet<ReadingChallenge> ReadingChallenges { get; set; }
        public DbSet<Space> Spaces { get; set; }
        public DbSet<Participant> Participants { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // <<< هذا السطر يجب أن يكون الأول دائماً

            // --- Book Entity Configuration ---
            // (يفضل تجميع الـ Configurations لكل Entity مع بعضها)
            modelBuilder.Entity<Book>(entity =>
            {
                // علاقة Book مع ApplicationUser (Uploader)
                entity.HasOne(b => b.Uploader)
                      .WithMany() // ApplicationUser ليس بالضرورة لديه Collection من UploadedBooks هنا
                      .HasForeignKey(b => b.UploadedBy)
                      .IsRequired(false) // UploadedBy هو Nullable
                      .OnDelete(DeleteBehavior.SetNull); // إذا حذف المستخدم، نجعل UploadedBy = NULL
            });

            // --- UserNote Entity Configuration ---
            modelBuilder.Entity<UserNote>(entity =>
            {
                entity.HasOne(un => un.User)
                    .WithMany() // ApplicationUser ليس بالضرورة لديه Collection من UserNotes
                    .HasForeignKey(un => un.UserID)
                    .IsRequired()
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(un => un.Book)
                    .WithMany() // Book ليس بالضرورة لديه Collection من UserNotes
                    .HasForeignKey(un => un.BookID)
                    .IsRequired(false)
                    .OnDelete(DeleteBehavior.ClientSetNull); // أو NoAction أو Restrict

                entity.HasOne(un => un.Chapter)
                    .WithMany() // Chapter ليس بالضرورة لديه Collection من UserNotes
                    .HasForeignKey(un => un.ChapterID)
                    .IsRequired(false)
                    .OnDelete(DeleteBehavior.ClientSetNull); // أو NoAction أو Restrict
            });

            // --- UserBookRating Entity Configuration ---
            modelBuilder.Entity<UserBookRating>(entity =>
            {
                // Primary Key مركب (أو RatingID كـ PK عادي، حسب تصميمك الأخير)
                // إذا كان RatingID هو الـ PK:
                entity.HasKey(ubr => ubr.RatingID);
                // وإذا أردت أن يكون UserID و BookID فريدين معاً (المستخدم يقيم الكتاب مرة واحدة):
                entity.HasIndex(ubr => new { ubr.UserID, ubr.BookID }).IsUnique();


                entity.HasOne(ubr => ubr.User)
                    .WithMany(u => u.BookRatings) // ApplicationUser.cs فيه ICollection<UserBookRating> BookRatings
                    .HasForeignKey(ubr => ubr.UserID)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(ubr => ubr.Book)
                    .WithMany(b => b.Ratings) // Book.cs فيه ICollection<UserBookRating> Ratings
                    .HasForeignKey(ubr => ubr.BookID)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // --- Chapter Entity Configuration ---
            modelBuilder.Entity<Chapter>(entity => {
                entity.HasOne(c => c.Book)
                      .WithMany(b => b.Chapters) // Book.cs فيه ICollection<Chapter> Chapters
                      .HasForeignKey(c => c.BookID)
                      .OnDelete(DeleteBehavior.Cascade); // إذا حذف الكتاب، تحذف فصوله
            });

            // --- Summary Entity Configuration ---
            modelBuilder.Entity<Summary>(entity => {
                entity.HasOne(s => s.Book)
                      .WithMany(b => b.Summaries) // Book.cs فيه ICollection<Summary> Summaries
                      .HasForeignKey(s => s.BookID)
                      .IsRequired(false) // BookID هو Nullable
                      .OnDelete(DeleteBehavior.Cascade); // أو ClientSetNull إذا أردت بقاء الملخص

                entity.HasOne(s => s.Chapter)
                      .WithMany() // Chapter لا يحتاج Collection للـ Summaries
                      .HasForeignKey(s => s.ChapterID)
                      .IsRequired(false) // ChapterID هو Nullable
                      .OnDelete(DeleteBehavior.Restrict); // أو ClientSetNull

                entity.HasOne(s => s.User)
                      .WithMany() // User لا يحتاج Collection للـ Summaries
                      .HasForeignKey(s => s.UserID)
                      .IsRequired(false) // UserID هو Nullable
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // --- Quiz, Question, Answer Configurations ---
            // (بافتراض العلاقات كما هي من قبل)
            modelBuilder.Entity<Quiz>(entity => {
                entity.HasOne(q => q.Book).WithMany(b => b.Quizzes).HasForeignKey(q => q.BookID).IsRequired(false).OnDelete(DeleteBehavior.ClientSetNull); // أو Cascade
                entity.HasOne(q => q.Chapter).WithMany(c => c.Quizzes).HasForeignKey(q => q.ChapterID).IsRequired(false).OnDelete(DeleteBehavior.ClientSetNull); // أو Cascade
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


            modelBuilder.Entity<Progress>(entity =>
            {
                entity.HasIndex(p => new { p.UserID, p.BookID }).IsUnique();

                entity.HasOne(p => p.User)
                    .WithMany() // ApplicationUser.cs لا يحتاج لـ ICollection<Progress> Progresses بالضرورة
                    .HasForeignKey(p => p.UserID)
                    .IsRequired()
                    .OnDelete(DeleteBehavior.Cascade); // Keep CASCADE for User

                entity.HasOne(p => p.Book)
                    .WithMany(b => b.Progresses) // Book.cs يجب أن يحتوي على ICollection<Progress> Progresses
                    .HasForeignKey(p => p.BookID)
                    .IsRequired()
                    .OnDelete(DeleteBehavior.Restrict); // Change to RESTRICT to avoid cascade cycle

                entity.HasOne(p => p.LastReadChapter)
                    .WithMany() // Chapter لا يحتاج لـ ICollection<Progress>
                    .HasForeignKey(p => p.LastReadChapterID)
                    .IsRequired(false)
                    .OnDelete(DeleteBehavior.SetNull); // Keep SET NULL for Chapter
            });

            modelBuilder.Entity<UserDailyActivityLog>(entity =>
            {
                entity.HasIndex(ual => new { ual.UserID, ual.ActivityDate }).IsUnique();

                entity.HasOne(ual => ual.User)
                    .WithMany() // ApplicationUser لا يحتاج لـ ICollection<UserDailyActivityLog>
                    .HasForeignKey(ual => ual.UserID)
                    .IsRequired()
                    .OnDelete(DeleteBehavior.Cascade);
            });
            // --- نهاية Configurations للـ Progress والـ Activity Log ---


            // Configurations للجداول الأخرى (ReadingChallenge, Space, Participant)
            // تأكد من أن الـ OnDelete behaviors منطقية ولا تسبب Multiple Cascade Paths
            modelBuilder.Entity<ReadingChallenge>(entity =>
            {
                entity.HasIndex(rc => new { rc.UserID, rc.Year }).IsUnique();
                entity.HasOne(rc => rc.User).WithMany().HasForeignKey(rc => rc.UserID).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Space>(entity =>
            {
                // entity.HasKey(s => s.Id); // الـ Key بيتعرف بالـ Convention لو اسمه Id أو SpaceId
                entity.Property(s => s.Title).IsRequired().HasMaxLength(100);
                entity.HasOne(s => s.Host).WithMany().HasForeignKey(s => s.HostId).IsRequired().OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Participant>(entity =>
            {
                entity.HasKey(p => new { p.UserId, p.SpaceId }); // Composite Key
                entity.HasOne(p => p.Space).WithMany(s => s.Participants).HasForeignKey(p => p.SpaceId).IsRequired().OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(p => new { p.SpaceId, p.AgoraUid }).IsUnique(); // كان موجود
                entity.HasOne(p => p.User).WithMany().HasForeignKey(p => p.UserId).IsRequired().OnDelete(DeleteBehavior.Cascade); // كان موجود
                entity.Property(p => p.Role).HasConversion<string>().HasMaxLength(20); // كان موجود
            });
        }
    }
}