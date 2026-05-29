using Microsoft.EntityFrameworkCore;
using QuizTaker.Models;

namespace QuizTaker.Data;

public sealed class QuizTakerDbContext(DbContextOptions<QuizTakerDbContext> options) : DbContext(options)
{
    public DbSet<QuizAttempt> QuizAttempts => Set<QuizAttempt>();

    public DbSet<QuizAttemptAnswer> QuizAttemptAnswers => Set<QuizAttemptAnswer>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<QuizAttempt>(entity =>
        {
            entity.Property(attempt => attempt.Course).HasMaxLength(200);
            entity.Property(attempt => attempt.Title).HasMaxLength(300);
            entity.Property(attempt => attempt.Mode).HasMaxLength(30);
            entity.Property(attempt => attempt.SourceSummary).HasMaxLength(1000);
            entity.HasMany(attempt => attempt.Answers)
                .WithOne(answer => answer.Attempt)
                .HasForeignKey(answer => answer.QuizAttemptId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<QuizAttemptAnswer>(entity =>
        {
            entity.Property(answer => answer.QuestionText).HasMaxLength(2000);
            entity.Property(answer => answer.SelectedAnswer).HasMaxLength(2000);
            entity.Property(answer => answer.CorrectAnswer).HasMaxLength(2000);
            entity.Property(answer => answer.SourceQuizTitle).HasMaxLength(300);
        });
    }
}
