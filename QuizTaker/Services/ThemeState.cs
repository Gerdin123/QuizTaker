namespace QuizTaker.Services;

public sealed class ThemeState
{
    public event Action? Changed;

    private static readonly HashSet<string> Themes =
    [
        "blue-light",
        "blue-dark",
        "gray-light",
        "gray-dark",
        "rose-light",
        "rose-dark",
        "violet-light",
        "violet-dark",
        "forest-light",
        "forest-dark"
    ];

    public string CurrentTheme { get; private set; } = "blue-light";

    public string CssClass => $"theme-{CurrentTheme}";

    public void SetTheme(string theme)
    {
        var nextTheme = Normalize(theme);
        if (CurrentTheme == nextTheme)
        {
            return;
        }

        CurrentTheme = nextTheme;
        Changed?.Invoke();
    }

    private static string Normalize(string theme)
    {
        return theme switch
        {
            "light" => "blue-light",
            "dark" => "blue-dark",
            "gray" => "gray-light",
            "rose" => "rose-light",
            "violet" => "violet-dark",
            "forest" => "forest-dark",
            _ when Themes.Contains(theme) => theme,
            _ => "blue-light"
        };
    }
}
