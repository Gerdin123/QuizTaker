namespace QuizTaker.Models;

public sealed record QuizDefinition(
    string Id,
    string Course,
    string Title,
    string FilePath,
    IReadOnlyList<QuizQuestion> Questions);

public sealed record QuizQuestion(
    string Id,
    string QuizId,
    string QuizTitle,
    string Course,
    string Text,
    IReadOnlyList<string> Choices,
    string CorrectAnswer);

public sealed record QuizCatalog(IReadOnlyList<CourseGroup> Courses)
{
    public IReadOnlyList<QuizDefinition> AllQuizzes => Courses.SelectMany(course => course.Quizzes).ToList();
}

public sealed record CourseGroup(string Name, IReadOnlyList<QuizDefinition> Quizzes);
