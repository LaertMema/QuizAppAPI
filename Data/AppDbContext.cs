using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

using Microsoft.EntityFrameworkCore;
using Quiz_App_API.Data.Models;

namespace Quiz_App_API.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Quiz> Quizzes { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<Alternative> Alternatives { get; set; }
        public DbSet<Subject> Subjects { get; set; }
        public DbSet<QuizAttempt> QuizAttempts { get; set; }
        public DbSet<UserAnswer> UserAnswers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            //Configuration per secilin model vecmas
            // Quiz Configuration
            modelBuilder.Entity<Quiz>(entity =>
            {
                entity.HasKey(q => q.Id);

                entity.Property(q => q.Title)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(q => q.Description)
                    .HasMaxLength(1000);

                entity.Property(q => q.CreatedAt)
                    .IsRequired()
                    .HasDefaultValueSql("GETDATE()");

                // Quiz -> Creator (User) relationship
                entity.HasOne(q => q.Creator)
                    .WithMany(u => u.CreatedQuizzes)
                    .HasForeignKey(q => q.CreatorId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Quiz -> Subject relationship
                entity.HasOne(q => q.Subject)
                    .WithMany(s => s.Quizzes)
                    .HasForeignKey(q => q.SubjectId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Question Configuration
            modelBuilder.Entity<Question>(entity =>
            {
                entity.HasKey(q => q.Id);

                entity.Property(q => q.Text)
                    .IsRequired()
                    .HasMaxLength(1000);

                // Question -> Quiz relationship
                entity.HasOne(q => q.Quiz)
                    .WithMany(q => q.Questions)
                    .HasForeignKey(q => q.QuizId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Option Configuration
            modelBuilder.Entity<Alternative>(entity =>
            {
                entity.HasKey(o => o.Id);

                entity.Property(o => o.Text)
                    .IsRequired()
                    .HasMaxLength(500);

                entity.Property(o => o.IsCorrect)
                    .IsRequired();

                // Option -> Question relationship
                entity.HasOne(o => o.Question)
                    .WithMany(q => q.Alternatives)
                    .HasForeignKey(o => o.QuestionId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Subject Configuration
            modelBuilder.Entity<Subject>(entity =>
            {
                entity.HasKey(s => s.Id);

                entity.Property(s => s.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(s => s.Description)
                    .HasMaxLength(500);

                // Create unique index on Name
                entity.HasIndex(s => s.Name)
                    .IsUnique();
            });

            // QuizAttempt Configuration
            modelBuilder.Entity<QuizAttempt>(entity =>
            {
                entity.HasKey(qa => qa.Id);

                entity.Property(qa => qa.StartedAt)
                    .IsRequired()
                    .HasDefaultValueSql("GETDATE()");

                entity.Property(qa => qa.Score)
                    .HasDefaultValue(0);

                // QuizAttempt -> Quiz relationship
                entity.HasOne(qa => qa.Quiz)
                    .WithMany(q => q.Attempts)
                    .HasForeignKey(qa => qa.QuizId)
                    .OnDelete(DeleteBehavior.Restrict);

                // QuizAttempt -> User relationship
                entity.HasOne(qa => qa.User)
                    .WithMany(u => u.QuizAttempts)
                    .HasForeignKey(qa => qa.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // AttemptAnswer Configuration
            modelBuilder.Entity<UserAnswer>(entity =>
            {
                entity.HasKey(aa => aa.Id);

                // AttemptAnswer -> QuizAttempt relationship
                entity.HasOne(aa => aa.QuizAttempt)
                    .WithMany(qa => qa.Answers)
                    .HasForeignKey(aa => aa.QuizAttemptId)
                    .OnDelete(DeleteBehavior.Cascade);
                // AttemptAnswer -> Question relationship
                entity.HasOne(aa => aa.Question)
                    .WithMany()  // Question doesn't need to track AttemptAnswers
                    .HasForeignKey(aa => aa.QuestionId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Create composite index
                entity.HasIndex(aa => new { aa.QuizAttemptId, aa.QuestionId })
                    .IsUnique();
            });

            // Configure default schema
            modelBuilder.HasDefaultSchema("quiz");

            // Add any seed data
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Seed initial subjects
            modelBuilder.Entity<Subject>().HasData(
                new Subject
                {
                    Id = 1,
                    Name = "Mathematics",
                    Description = "Mathematics related quizzes"
                },
                new Subject
                {
                    Id = 2,
                    Name = "Science",
                    Description = "Science related quizzes"
                },
                new Subject
                {
                    Id = 3,
                    Name = "History",
                    Description = "History related quizzes"
                },
                new Subject
                {
                    Id = 4,
                    Name = "Geography",
                    Description = "Geography related quizzes"
                },
                new Subject
                {
                    Id = 5,
                    Name = "Programming",
                    Description = "Programming and Computer Science related quizzes"
                }
            );
        }
    }
}
