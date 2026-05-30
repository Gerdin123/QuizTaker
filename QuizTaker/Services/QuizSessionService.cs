using Microsoft.EntityFrameworkCore;
using QuizTaker.Data;
using QuizTaker.Models;

namespace QuizTaker.Services;

public sealed class QuizSessionService(IServiceScopeFactory scopeFactory)
{
    private readonly Dictionary<Guid, QuizRun> runs = [];
    private readonly Dictionary<Guid, QuizRunResult> results = [];

    public Guid StartQuiz(QuizDefinition quiz)
    {
        return StoreRun(new QuizRun
        {
            Mode = QuizRunMode.Quiz,
            Course = quiz.Course,
            Title = quiz.Title,
            SourceSummary = quiz.Title,
            Questions = quiz.Questions.ToList()
        });
    }

    public Guid StartPractice(string course, IReadOnlyList<QuizDefinition> quizzes)
    {
        return StoreRun(new QuizRun
        {
            Mode = QuizRunMode.Practice,
            Course = course,
            Title = $"{course} practice",
            SourceSummary = string.Join(", ", quizzes.Select(quiz => quiz.Title)),
            Questions = quizzes.SelectMany(quiz => quiz.Questions).ToList()
        });
    }

    public Guid StartExam(string course, IReadOnlyDictionary<QuizDefinition, int> questionCounts)
    {
        var questions = questionCounts
            .SelectMany(pair => pair.Key.Questions.OrderBy(_ => Random.Shared.Next()).Take(pair.Value))
            .OrderBy(_ => Random.Shared.Next())
            .ToList();

        return StoreRun(new QuizRun
        {
            Mode = QuizRunMode.Exam,
            Course = course,
            Title = $"{course} exam",
            SourceSummary = string.Join(", ", questionCounts.Select(pair => $"{pair.Value} from {pair.Key.Title}")),
            Questions = questions
        });
    }

    public QuizRun? GetRun(Guid runId)
    {
        return runs.GetValueOrDefault(runId);
    }

    public QuizRunResult? GetResult(Guid runId)
    {
        return results.GetValueOrDefault(runId);
    }

    public async Task<QuizRunResult?> CompleteRunAsync(Guid runId, IReadOnlyDictionary<string, string> selectedAnswers)
    {
        if (!runs.TryGetValue(runId, out var run))
        {
            return null;
        }

        var answers = run.Questions
            .Select(question => new QuizRunAnswer(
                question,
                selectedAnswers.TryGetValue(question.Id, out var selectedAnswer) ? selectedAnswer : null))
            .ToList();
        var completedAt = DateTimeOffset.UtcNow;
        var duration = completedAt - run.StartedAt;

        int? attemptId = null;
        if (run.ShouldSaveScore)
        {
            using var scope = scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<QuizTakerDbContext>();
            var attempt = new QuizAttempt
            {
                Course = run.Course,
                Title = run.Title,
                Mode = run.Mode.ToString(),
                SourceSummary = run.SourceSummary,
                CorrectCount = answers.Count(answer => answer.IsCorrect),
                TotalQuestions = answers.Count,
                CompletedAt = completedAt,
                Answers = answers.Select(answer => new QuizAttemptAnswer
                {
                    SourceQuizTitle = answer.Question.QuizTitle,
                    QuestionText = answer.Question.Text,
                    SelectedAnswer = answer.SelectedAnswer ?? string.Empty,
                    CorrectAnswer = answer.Question.CorrectAnswer,
                    IsCorrect = answer.IsCorrect
                }).ToList()
            };

            db.QuizAttempts.Add(attempt);
            await db.SaveChangesAsync();
            attemptId = attempt.Id;
        }

        var result = new QuizRunResult
        {
            RunId = run.Id,
            Mode = run.Mode,
            Course = run.Course,
            Title = run.Title,
            ScoreSaved = run.ShouldSaveScore,
            SavedAttemptId = attemptId,
            Duration = duration,
            Answers = answers
        };

        results[runId] = result;
        runs.Remove(runId);
        return result;
    }

    public async Task<IReadOnlyList<QuizAttempt>> GetRecentAttemptsAsync()
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<QuizTakerDbContext>();
        var attempts = await db.QuizAttempts
            .AsNoTracking()
            .ToListAsync();

        return attempts
            .OrderByDescending(attempt => attempt.CompletedAt)
            .Take(25)
            .ToList();
    }

    public async Task<QuizAttempt?> GetSavedAttemptAsync(int id)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<QuizTakerDbContext>();
        return await db.QuizAttempts
            .AsNoTracking()
            .Include(attempt => attempt.Answers)
            .FirstOrDefaultAsync(attempt => attempt.Id == id);
    }

    private Guid StoreRun(QuizRun run)
    {
        runs[run.Id] = run;
        return run.Id;
    }
}
