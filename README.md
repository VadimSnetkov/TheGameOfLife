# TheGameOfLife

Zero-player cellular automaton based on John Horton Conway game.

## Current features — Iteration 3

- Run 1–1,000 independent games with parallel generation updates.
- Choose small, medium, large, or custom boards: 1–100 rows and 1–200 columns.
- Each cell starts with a 30% chance of being alive.
- Display up to eight selected games, identified by permanent numbers within the session.
- Change the displayed games while every game continues evolving.
- Show the total game count, live game count, total living cells, and individual generation counts.
- Save all games together and restore their boards, numbers, and iteration counts at startup.
- Load existing Iteration 2 single-game saves.
- Validate input and saved data with FluentValidation.
- Stop while updating or saving; failed or cancelled saves preserve the previous completed file.

A **live game** has at least one living cell. Stable patterns and oscillators still count as live.
Cells outside a board are dead. Updates create new boards without changing the previous generation.

## Projects

- **GameOfLife.Core** — models, game rules, parallel session evolution, statistics, persistence, and validation. Contains no console calls.
- **GameOfLife.ConsoleApp** — startup menu, interactive dashboard, view selection, configuration, and dependency injection.
- **GameOfLife.Tests** — xUnit tests for Core and the dashboard's selection and layout behavior.

Models hold data. Services operate on that data. Each class has its own file.

## Run and test

Requires the **.NET 10 SDK**. Run from the repository root:

```powershell
dotnet build
dotnet test
dotnet run --project src/GameOfLife.ConsoleApp
```

Use an interactive terminal. A window of **120 columns × 40 rows** is comfortable for eight small boards.
The dashboard also works at 80 × 24. When a board cannot fit, its panel shows the top-left area and labels
that area as `View rows x columns`. All cells still participate in the simulation and statistics.
Select fewer games or enlarge the terminal to see more of each board.

## Controls

| Context | Key | Action |
| --- | --- | --- |
| Startup | 1 / 2 / 3 | Choose a preset board size, then enter the number of games |
| Startup | 4 | Enter custom dimensions and the number of games |
| Startup | 5 | Load an Iteration 2 single-game save |
| Startup | 6 | Load a saved session containing all games |
| Startup | 0 | Exit |
| Dashboard | V | Enter 1–8 distinct game numbers separated by commas, then press Enter |
| Dashboard | S | Save all games, including games outside the selected views |
| Dashboard | Q / Esc | Exit |
| View editor | Esc | Cancel selection changes |
| View editor | Q | Exit |
| Anywhere | Ctrl+C | Stop the application |

For example, press `V`, type `1,7,42,1000`, and press Enter in a 1,000-game session.
Editing the selection and saving do not pause evolution. Saving is explicit: exiting does not automatically save.
A pending save is cancelled when exiting; wait for the success message if you want to keep it.

## Save files and configuration

`src/GameOfLife.ConsoleApp/appsettings.json` configures the folder and file names:

```json
{
  "GameSave": {
    "Folder": "TheGameOfLife",
    "FileName": "game.json",
    "SessionFileName": "games.json"
  }
}
```

On Windows, the default directory is `%LOCALAPPDATA%\TheGameOfLife`.
`games.json` contains all games and is separate from the older `game.json` file.
The game number is its position in the saved array plus one. Loading keeps that order.
Views initially show the first eight games; the selection itself is not saved.

A save captures one completed session snapshot. Every board and its iteration count belong to that snapshot,
even if another generation finishes while the file is being written. Files are validated before use and
written to a temporary file before replacing the previous save.

## How parallel updates work

`GameService` advances one game. `SessionService` uses `Parallel.For` to advance every game in a session,
with concurrency limited to the processor count. It creates a new session only after all games finish.
The runner keeps the old session available for rendering and saving until then.

The console runs generation work in a background task and polls keys approximately every 50 milliseconds.
It aims to start a generation once per second, with only one generation calculation active at a time.
Large workloads can take longer than one second depending on the computer; updates never overlap or queue indefinitely.

## Verification

The suite includes 97 test cases, covering sequential versus parallel results for 1,000 games,
all-game statistics, eight-view layout, selection validation, save/load correctness, cancellation,
old-save compatibility, and saving/loading 1,000 maximum-size boards.
