# QuizTaker

QuizTaker is a single-project Blazor Server app for taking saved quiz questions, combining quizzes for practice, and creating scored exams.

## Features

- Reads copied quiz text files from `QuizTaker/Quizzes`.
- Uses subfolder names as course/topic names.
- Groups root-level quiz files under `Other`.
- Lets users take a single quiz and saves the score.
- Lets users combine quizzes from the same course/topic as an unsaved practice test.
- Provides a dedicated exam page where quizzes are selected by default and each quiz defaults to 10 exam questions.
- Shows result summaries with correct count, percentage, and answer review.
- Saves quiz and exam attempts with EF Core and SQLite.
- Includes light and dark themes with persisted theme preference.
- Uses a custom app shell instead of the default Blazor template UI.

## Project Structure

```text
QuizTaker.slnx
QuizTaker/
  Components/
    Layout/
    Pages/
    ResultSummary.razor
    ThemeToggle.razor
  Data/
    QuizTakerDbContext.cs
  Models/
  Quizzes/
  Services/
  wwwroot/
  Program.cs
  QuizTaker.csproj
```

## Quiz File Layout

Place quiz files under `QuizTaker/Quizzes`.

```text
QuizTaker/Quizzes/
  DevOps/
    DevOps Philosophy and Concepts.txt
    Infrastructure as Code.txt
  Some root quiz.txt
```

This produces:

- Course/topic `DevOps` for files under `Quizzes/DevOps`.
- Course/topic `Other` for `Some root quiz.txt`.

The app currently parses copied multiple-choice quiz result text that includes question numbers, `Multiple Choice`, answer options, and `Correct answer:` markers.

## Run

```powershell
dotnet run --project QuizTaker\QuizTaker.csproj
```

Default local URL:

```text
http://localhost:5092
```

## Build

```powershell
dotnet build QuizTaker.slnx
```

## App Flow

Home page:

- Start a single quiz from a quiz card.
- Click `Start combined test` on a course/topic to enter selection mode.
- In combined selection mode, all quizzes for that course are selected by default.
- Click quiz cards to include or exclude them.
- Combined tests are practice only and are not saved.

Exam page:

- Navigate to `/exam`.
- All quizzes are selected by default.
- Each quiz defaults to 10 questions, capped by the number of available questions.
- Click quiz cards to include or exclude them.
- Exam scores are saved.

## Theme

The app supports light and dark mode.

- The selected theme is stored in browser `localStorage` under `quiztaker-theme`.
- `Components/ThemeToggle.razor` controls the UI state.
- `Services/ThemeState.cs` keeps the Blazor layout theme state.
- `wwwroot/theme.js` applies the saved browser theme before render.

App links opt out of Blazor enhanced navigation with `data-enhance-nav="false"`. This is intentional: enhanced navigation was replacing page markup before the saved theme was reapplied, causing dark mode to visually reset until a manual refresh.

## Data

The app creates a local SQLite database at:

```text
QuizTaker/quiztaker.db
```

This database stores saved quiz and exam attempts. It is ignored by git.

ASP.NET Data Protection keys are stored at:

```text
QuizTaker/DataProtectionKeys
```

These are runtime-local and ignored by git.
