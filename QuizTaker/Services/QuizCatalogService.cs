using System.Text.RegularExpressions;
using QuizTaker.Models;

namespace QuizTaker.Services;

public sealed partial class QuizCatalogService(IWebHostEnvironment environment)
{
    private QuizCatalog? catalog;

    public QuizCatalog GetCatalog()
    {
        if (catalog is not null)
        {
            return catalog;
        }

        var quizzesRoot = Path.Combine(environment.ContentRootPath, "Quizzes");
        if (!Directory.Exists(quizzesRoot))
        {
            catalog = new QuizCatalog([]);
            return catalog;
        }

        var quizzes = Directory
            .EnumerateFiles(quizzesRoot, "*.txt", SearchOption.AllDirectories)
            .Select(file => LoadQuiz(quizzesRoot, file))
            .Where(quiz => quiz.Questions.Count > 0)
            .OrderBy(quiz => quiz.Course)
            .ThenBy(quiz => quiz.Title)
            .ToList();

        catalog = new QuizCatalog(quizzes
            .GroupBy(quiz => quiz.Course)
            .Select(group => new CourseGroup(group.Key, group.ToList()))
            .OrderBy(group => group.Name)
            .ToList());

        return catalog;
    }

    public QuizDefinition? FindQuiz(string id)
    {
        return GetCatalog().AllQuizzes.FirstOrDefault(quiz => quiz.Id == id);
    }

    private static QuizDefinition LoadQuiz(string quizzesRoot, string file)
    {
        var relativePath = Path.GetRelativePath(quizzesRoot, file);
        var segments = relativePath.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var course = segments.Length > 1 ? segments[0] : "Other";
        var title = Path.GetFileNameWithoutExtension(file);
        var id = ToStableId(relativePath);
        var text = File.ReadAllText(file);
        var questions = ParseQuestions(text, id, title, course);

        return new QuizDefinition(id, course, title, file, questions);
    }

    private static IReadOnlyList<QuizQuestion> ParseQuestions(string text, string quizId, string quizTitle, string course)
    {
        var normalizedLines = text
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace('\r', '\n')
            .Split('\n')
            .Select(line => line.Trim())
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .ToList();

        var numberIndexes = normalizedLines
            .Select((line, index) => new { line, index })
            .Where(item => QuestionNumberRegex().IsMatch(item.line))
            .Select(item => item.index)
            .ToList();

        var questions = new List<QuizQuestion>();
        for (var i = 0; i < numberIndexes.Count; i++)
        {
            var start = numberIndexes[i];
            var end = i + 1 < numberIndexes.Count ? numberIndexes[i + 1] : normalizedLines.Count;
            var block = normalizedLines.Skip(start).Take(end - start).ToList();
            var question = ParseQuestionBlock(block, quizId, quizTitle, course, questions.Count + 1);
            if (question is not null)
            {
                questions.Add(question);
            }
        }

        return questions;
    }

    private static QuizQuestion? ParseQuestionBlock(
        IReadOnlyList<string> block,
        string quizId,
        string quizTitle,
        string course,
        int questionNumber)
    {
        var typeIndex = block.ToList().FindIndex(line => line.Equals("Multiple Choice", StringComparison.OrdinalIgnoreCase));
        var correctMarkerIndex = block.ToList().FindIndex(line => line.Equals("Correct answer:", StringComparison.OrdinalIgnoreCase));

        if (typeIndex < 0 || correctMarkerIndex < 0 || correctMarkerIndex + 1 >= block.Count)
        {
            return null;
        }

        var questionText = typeIndex + 1 < block.Count ? block[typeIndex + 1].Trim() : string.Empty;
        var correctAnswer = CorrectKnownSourceAnswer(questionText, block[correctMarkerIndex + 1].Trim());
        if (string.IsNullOrWhiteSpace(questionText) || string.IsNullOrWhiteSpace(correctAnswer))
        {
            return null;
        }

        var options = block
            .Skip(typeIndex + 1)
            .Where(line => !IsAnswerFeedbackMarker(line))
            .Where(line => !line.Equals(", Not Selected", StringComparison.OrdinalIgnoreCase))
            .Where(line => !line.StartsWith("Results for question ", StringComparison.OrdinalIgnoreCase))
            .Where(line => !line.Equals(questionText, StringComparison.Ordinal))
            .Where(line => !QuestionNumberRegex().IsMatch(line))
            .Where(line => !PointLineRegex().IsMatch(line))
            .Where(line => !line.Equals("Multiple Choice", StringComparison.OrdinalIgnoreCase))
            .Select(line => line.Trim())
            .Where(line => line.Length > 0)
            .Distinct(StringComparer.Ordinal)
            .ToList();

        if (!options.Contains(correctAnswer, StringComparer.Ordinal))
        {
            options.Add(correctAnswer);
        }

        return new QuizQuestion(
            $"{quizId}:{questionNumber}",
            quizId,
            quizTitle,
            course,
            questionText,
            options,
            correctAnswer);
    }

    private static bool IsAnswerFeedbackMarker(string line)
    {
        return line.Equals("Correct answer:", StringComparison.OrdinalIgnoreCase)
            || line.Equals("Incorrect answer:", StringComparison.OrdinalIgnoreCase);
    }

    private static string CorrectKnownSourceAnswer(string questionText, string correctAnswer)
    {
        if (questionText.Equals("How does Terraform ensure idempotency?", StringComparison.OrdinalIgnoreCase)
            || questionText.Equals("How does Terraform enusre idempotency?", StringComparison.OrdinalIgnoreCase))
        {
            return "By using the state file to track resources";
        }

        if (questionText.Equals("What happens if a job fails in GitHub Actions?", StringComparison.OrdinalIgnoreCase))
        {
            return "The entire workflow fails by default";
        }

        return correctAnswer;
    }

    private static string ToStableId(string value)
    {
        return Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(value)))[..16];
    }

    [GeneratedRegex(@"^\d+$")]
    private static partial Regex QuestionNumberRegex();

    [GeneratedRegex(@"^\d+\s*/\s*\d+\s*point")]
    private static partial Regex PointLineRegex();
}
