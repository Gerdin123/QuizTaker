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

If a build fails because `QuizTaker.exe` or `QuizTaker.dll` is locked, a previous dev server is probably still running. Stop the `QuizTaker` or related `dotnet` process before rebuilding. If you only need compile verification while a server is running, build to a temporary output folder:

```powershell
dotnet build QuizTaker\QuizTaker.csproj -o _tmp_verify_build /p:UseAppHost=false
```

## Architecture

- `Program.cs` configures Blazor Server, EF Core SQLite, Data Protection key storage, and app services.
- `Services/QuizCatalogService.cs` scans and parses quiz text files from `Quizzes`.
- `Services/QuizSessionService.cs` manages active quiz runs, elapsed timing, completion, scoring, and saved attempt lookup.
- `Data/QuizTakerDbContext.cs` defines EF Core persistence for saved attempts.
- `Components/Pages/Home.razor` handles single quizzes and combined practice tests.
- `Components/Pages/Exam.razor` handles scored exam setup.
- `Components/Pages/TakeQuiz.razor` renders active quiz runs, selected answer highlighting, elapsed time, answered count, and final submit controls.
- `Components/Pages/Results.razor` and `Components/ResultSummary.razor` show immediate results.
- `Components/Pages/AttemptDetails.razor` shows saved scores.
- `Components/ThemeToggle.razor`, `Services/ThemeState.cs`, and `wwwroot/theme.js` handle persisted theme selection.

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

The parser treats copied feedback labels such as `Correct answer:` and `Incorrect answer:` as metadata, not answer choices. Preserve the actual answer text following those labels.

Keep parser changes conservative and test against the existing files in `QuizTaker/Quizzes`.

## UI Notes

The app intentionally does not use the default Blazor template UI or Bootstrap. Global styling lives in:

```text
QuizTaker/wwwroot/app.css
```

Keep card layouts stable: quiz cards use grid rows so controls align even when titles have different lengths.

Theme behavior:

- The current theme is stored in browser `localStorage` as `quiztaker-theme`.
- Supported current theme values are `blue-light`, `blue-dark`, `gray-light`, `gray-dark`, `rose-light`, `rose-dark`, `violet-light`, `violet-dark`, `forest-light`, and `forest-dark`.
- Legacy values `light`, `dark`, `gray`, `rose`, `violet`, and `forest` are mapped to the closest current theme.
- `theme.js` applies the saved theme to document-level attributes/classes before render.
- `ThemeState` also applies the theme class to `.app-shell`.
- The theme dropdown is styled with app CSS variables and an explicit per-theme control color scheme so it does not follow the client OS setting.
- Keep app route links using `data-enhance-nav="false"` unless the theme implementation is changed. Enhanced navigation caused page transitions to visually reset the active theme until a browser refresh.

Quiz-taking UI:

- Selected answers should highlight the full `.choice-row`, not only the radio input.
- The elapsed timer and answered count are shown above the question list in `.run-metrics`.
- The submit bar is a normal final page section, not sticky, so it does not cover following questions.

Navigation behavior:

- Normal links between app pages should opt out of enhanced navigation.
- Programmatic navigation from quiz/exam submission uses `forceLoad: true` for the same reason.
- If changing navigation behavior, manually test: switch themes, navigate from `/` to `/exam`, and confirm the page keeps the selected theme without pressing Ctrl+R.
