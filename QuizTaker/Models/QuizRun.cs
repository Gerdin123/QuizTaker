namespace QuizTaker.Models;

public enum QuizRunMode
{
    Quiz,
    Practice,
    Exam
}

public sealed class QuizRun
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public QuizRunMode Mode { get; init; }

    public string Course { get; init; } = string.Empty;

    public string Title { get; init; } = string.Empty;

    public string SourceSummary { get; init; } = string.Empty;

    public bool ShouldSaveScore => Mode is QuizRunMode.Quiz or QuizRunMode.Exam;

    public List<QuizQuestion> Questions { get; init; } = [];
}

public sealed record QuizRunAnswer(QuizQuestion Question, string? SelectedAnswer)
{
    public bool IsCorrect => string.Equals(SelectedAnswer, Question.CorrectAnswer, StringComparison.Ordinal);
}

public sealed class QuizRunResult
{
    public Guid RunId { get; init; }

    public QuizRunMode Mode { get; init; }

    public string Course { get; init; } = string.Empty;

    public string Title { get; init; } = string.Empty;

    public bool ScoreSaved { get; init; }

    public int? SavedAttemptId { get; init; }

    public List<QuizRunAnswer> Answers { get; init; } = [];

    public int CorrectCount => Answers.Count(answer => answer.IsCorrect);

    public int TotalQuestions => Answers.Count;

    public decimal Percentage => TotalQuestions == 0 ? 0 : Math.Round((decimal)CorrectCount / TotalQuestions * 100, 1);
}
