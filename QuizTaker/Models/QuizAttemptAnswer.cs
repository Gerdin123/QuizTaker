namespace QuizTaker.Models;

public sealed class QuizAttemptAnswer
{
    public int Id { get; set; }

    public int QuizAttemptId { get; set; }

    public QuizAttempt? Attempt { get; set; }

    public string SourceQuizTitle { get; set; } = string.Empty;

    public string QuestionText { get; set; } = string.Empty;

    public string SelectedAnswer { get; set; } = string.Empty;

    public string CorrectAnswer { get; set; } = string.Empty;

    public bool IsCorrect { get; set; }
}
