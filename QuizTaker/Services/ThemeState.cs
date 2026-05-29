namespace QuizTaker.Services;

public sealed class ThemeState
{
    public event Action? Changed;

    public string CurrentTheme { get; private set; } = "light";

    public string CssClass => CurrentTheme == "dark" ? "theme-dark" : "theme-light";

    public void SetTheme(string theme)
    {
        var nextTheme = theme == "dark" ? "dark" : "light";
        if (CurrentTheme == nextTheme)
        {
            return;
        }

        CurrentTheme = nextTheme;
        Changed?.Invoke();
    }
}
