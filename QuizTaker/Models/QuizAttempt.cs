namespace QuizTaker.Models;

public sealed class QuizAttempt
{
    public int Id { get; set; }

    public string Course { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Mode { get; set; } = string.Empty;

    public string SourceSummary { get; set; } = string.Empty;

    public int CorrectCount { get; set; }

    public int TotalQuestions { get; set; }

    public DateTimeOffset CompletedAt { get; set; } = DateTimeOffset.UtcNow;

    public List<QuizAttemptAnswer> Answers { get; set; } = [];
}
