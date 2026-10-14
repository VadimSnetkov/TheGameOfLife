# TheGameOfLife

Zero-player cellular automaton based on John Horton Conway game.

## Current features — Iteration 2

- Small, medium, large, or custom boards (1–100 rows and 1–200 columns).
- A random starting layout with a 30% chance of each cell being alive.
- Conway's rules applied approximately once per second.
- Iteration and living-cell counters. A new game starts at iteration zero.
- Save the current board and iteration to JSON, then load them at startup.
- Save-file validation and readable errors for missing, damaged, or inaccessible files.
- Responsive keyboard controls for saving and stopping.

Cells outside the board are treated as dead. Each generation is calculated on a separate board.

## Projects

- **GameOfLife.Core** — board and game-state models, game rules, board creation, statistics, file storage, and board/save validation. Contains no console calls.
- **GameOfLife.ConsoleApp** — menu, keyboard controls, rendering, and application setup. References Core.
- **GameOfLife.Tests** — xUnit tests referencing Core, including game rules, custom dimensions, statistics, and save/load behavior.

Models hold data. Services operate on that data. Each class is in its own file.

## Run and test

Requires the **.NET 10 SDK**. Run these commands from the repository root:

```powershell
dotnet build
dotnet test
dotnet run --project src/GameOfLife.ConsoleApp
```

Run in an interactive terminal such as Visual Studio's console or PowerShell.

## Controls

| Context | Key | Action |
| --- | --- | --- |
| Startup menu | 1 / 2 / 3 | Start a preset board |
| Startup menu | 4 | Enter custom rows and columns |
| Startup menu | 5 | Load the saved game |
| Startup menu | 0 | Exit |
| Running game | S | Save the displayed state; replace the previous save |
| Running game | Q / Esc | Exit |
| Anywhere | Ctrl+C | Stop the application |

Saving is explicit: exiting does not automatically save. The latest successful save remains available after restarting.

On Windows, the save file is `%LOCALAPPDATA%\TheGameOfLife\game.json`. The startup menu also displays its full path. The same path is used regardless of the directory from which the app is launched.

## Implementation

- `GameState` holds the board and iteration number.
- `GameService.NextIterationStep(GameState)` returns the next state while preserving the previous one.
- `BoardService.CountLivingCells` calculates the population from the actual board.
- `GameFileService` writes a separate temporary file before replacing the previous save.
- `GameSaveData` stores cells in a flat array, row by row, for JSON serialization.
- FluentValidation checks custom sizes and saved data before a board is restored.
- The console checks keys every 50 milliseconds and uses elapsed time to update the board about once a second.

## Next iteration

Run multiple games in parallel, select games to display, save all games, and show combined statistics.
