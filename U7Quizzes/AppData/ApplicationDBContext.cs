using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using U7Quizzes.Models;

namespace U7Quizzes.AppData
{
    public class ApplicationDBContext : IdentityDbContext<User>
    {
        public DbSet<User> User { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        // --------------------------
        public DbSet<Favorite> Favorite { get; set; }
        public DbSet<Quiz> Quiz { get; set; }
        public DbSet<Question> Question { get; set; }
        public DbSet<Answer> Answer { get; set; }
        public DbSet<Response> Response { get; set; }
        // --------------------------
        public DbSet<Category> Category { get; set; }
        public DbSet<QuizCategory> QuizCategory { get; set; }
        // --------------------------
        public DbSet<Tag> Tag { get; set; }
        public DbSet<QuizTag> QuizTag { get; set; }
        // --------------------------
        public DbSet<Session> Session { get; set; }
        public DbSet<SessionReport> SessionReport { get; set; }
        // --------------------------
        public DbSet<Participant> Participant { get; set; }


        public ApplicationDBContext(DbContextOptions<ApplicationDBContext> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


          



            // User
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Id)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // Quiz
            modelBuilder.Entity<Quiz>()
                .HasOne(q => q.Creator)
                .WithMany(u => u.Quizzes)
                .HasForeignKey(q => q.CreatorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Question
            modelBuilder.Entity<Question>()
                .HasOne(q => q.Quiz)
                .WithMany(q => q.Questions)
                .HasForeignKey(q => q.QuizId)
                .OnDelete(DeleteBehavior.Cascade); // OK nếu Quiz bị xóa thì xóa luôn Question

            // Answer
            modelBuilder.Entity<Answer>()
                .HasOne(a => a.Question)
                .WithMany(q => q.Answers)
                .HasForeignKey(a => a.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);

            // Session
            modelBuilder.Entity<Session>()
                .HasOne(s => s.Quiz)
                .WithMany(q => q.Sessions)
                .HasForeignKey(s => s.QuizId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Session>()
                .HasOne(s => s.Host)
                .WithMany(u => u.HostedSessions)
                .HasForeignKey(s => s.HostId)
                .OnDelete(DeleteBehavior.Restrict);

            // Participant
            modelBuilder.Entity<Participant>()
                .HasOne(p => p.Session)
                .WithMany(s => s.Participants)
                .HasForeignKey(p => p.SessionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Participant>()
                .HasOne(p => p.User)
                .WithMany(u => u.Participations)
                .HasForeignKey(p => p.UserId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict); // Cẩn thận tránh cascade nếu User bị xóa

            // Response
            modelBuilder.Entity<Response>()
                .HasOne(r => r.Participant)
                .WithMany(p => p.Responses)
                .HasForeignKey(r => r.ParticipantId)
                .OnDelete(DeleteBehavior.Restrict); // ✨ Giải quyết multiple cascade path

            modelBuilder.Entity<Response>()
                .HasOne(r => r.Question)
                .WithMany(q => q.Responses)
                .HasForeignKey(r => r.QuestionId)
                .OnDelete(DeleteBehavior.Restrict); // ✨ Giải quyết multiple cascade path

            modelBuilder.Entity<Response>()
                .HasOne(r => r.Answer)
                .WithMany(a => a.Responses)
                .HasForeignKey(r => r.AnswerId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict); // ✨ Tránh cascade rắc rối

            // Category
            modelBuilder.Entity<QuizCategory>()
                .HasKey(qc => qc.QuizCategoryId);

            modelBuilder.Entity<QuizCategory>()
                .HasOne(qc => qc.Quiz)
                .WithMany(q => q.QuizCategories)
                .HasForeignKey(qc => qc.QuizId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<QuizCategory>()
                .HasOne(qc => qc.Category)
                .WithMany(c => c.QuizCategories)
                .HasForeignKey(qc => qc.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<QuizCategory>()
                .HasIndex(qc => new { qc.QuizId, qc.CategoryId })
                .IsUnique();

            // Tag
            modelBuilder.Entity<QuizTag>()
                .HasKey(qt => qt.QuizTagId);

            modelBuilder.Entity<QuizTag>()
                .HasOne(qt => qt.Quiz)
                .WithMany(q => q.QuizTags)
                .HasForeignKey(qt => qt.QuizId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<QuizTag>()
                .HasOne(qt => qt.Tag)
                .WithMany(t => t.QuizTags)
                .HasForeignKey(qt => qt.TagId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<QuizTag>()
                .HasIndex(qt => new { qt.QuizId, qt.TagId })
                .IsUnique();

            // Favorite
            modelBuilder.Entity<Favorite>()
                .HasKey(f => f.FavoriteId);

            modelBuilder.Entity<Favorite>()
                .HasOne(f => f.User)
                .WithMany(u => u.Favorites)
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Favorite>()
                .HasOne(f => f.Quiz)
                .WithMany(q => q.Favorites)
                .HasForeignKey(f => f.QuizId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Favorite>()
                .HasIndex(f => new { f.UserId, f.QuizId })
                .IsUnique();

            // SessionReport
            modelBuilder.Entity<SessionReport>()
                .HasOne(sr => sr.Session)
                .WithOne(s => s.Report)
                .HasForeignKey<SessionReport>(sr => sr.SessionId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure ENUM values
            modelBuilder.Entity<Question>()
                .Property(q => q.Type)
                .HasConversion<string>()
                .HasMaxLength(20);

            modelBuilder.Entity<Session>()
                .Property(s => s.Status)
                .HasConversion<string>()
                .HasMaxLength(20);
        }

    }

}
