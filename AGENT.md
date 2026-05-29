# Agent Notes

## Project

This is a single-project Blazor Server app targeting `.NET 10`.

Main project:

```text
QuizTaker/QuizTaker.csproj
```

Solution:

```text
QuizTaker.slnx
```

## Run and Verify

Build:

```powershell
dotnet build QuizTaker.slnx
```

Run:

```powershell
dotnet run --project QuizTaker\QuizTaker.csproj
```

Default URL:

```text
http://localhost:5092
```

If a build fails because `QuizTaker.exe` is locked, a previous dev server is probably still running. Stop the `QuizTaker` or related `dotnet` process before rebuilding.

## Architecture

- `Program.cs` configures Blazor Server, EF Core SQLite, Data Protection key storage, and app services.
- `Services/QuizCatalogService.cs` scans and parses quiz text files from `Quizzes`.
- `Services/QuizSessionService.cs` manages active quiz runs, completion, scoring, and saved attempt lookup.
- `Data/QuizTakerDbContext.cs` defines EF Core persistence for saved attempts.
- `Components/Pages/Home.razor` handles single quizzes and combined practice tests.
- `Components/Pages/Exam.razor` handles scored exam setup.
- `Components/Pages/TakeQuiz.razor` renders active quiz runs.
- `Components/Pages/Results.razor` and `Components/ResultSummary.razor` show immediate results.
- `Components/Pages/AttemptDetails.razor` shows saved scores.
- `Components/ThemeToggle.razor`, `Services/ThemeState.cs`, and `wwwroot/theme.js` handle light/dark theme switching.

## Runtime Files

Do not commit runtime state:

- `QuizTaker/quiztaker.db`
- `QuizTaker/quiztaker.db-*`
- `QuizTaker/DataProtectionKeys/`
- logs
- `bin/`
- `obj/`
- `.vs/`

## Quiz Parsing

Quiz files are expected under:

```text
QuizTaker/Quizzes/**/*.txt
```

Subfolder name becomes the course/topic. Files directly under `Quizzes` are categorized as `Other`.

The current parser expects copied multiple-choice quiz text with:

- question number lines
- `Multiple Choice`
- answer options
- `Correct answer:`

Keep parser changes conservative and test against the existing files in `QuizTaker/Quizzes`.

## UI Notes

The app intentionally does not use the default Blazor template UI or Bootstrap. Global styling lives in:

```text
QuizTaker/wwwroot/app.css
```

Keep card layouts stable: quiz cards use grid rows so controls align even when titles have different lengths.

Theme behavior:

- The current theme is stored in browser `localStorage` as `quiztaker-theme`.
- `theme.js` applies the saved theme to document-level attributes/classes before render.
- `ThemeState` also applies the theme class to `.app-shell`.
- Keep app route links using `data-enhance-nav="false"` unless the theme implementation is changed. Enhanced navigation caused page transitions to visually reset to light mode until a browser refresh.

Navigation behavior:

- Normal links between app pages should opt out of enhanced navigation.
- Programmatic navigation from quiz/exam submission uses `forceLoad: true` for the same reason.
- If changing navigation behavior, manually test: switch to dark mode, navigate from `/` to `/exam`, and confirm the page stays dark without pressing Ctrl+R.
