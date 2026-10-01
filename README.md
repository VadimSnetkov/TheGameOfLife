# The Game Of Life

Zero-player cellular automaton based on John Horton Conway game.

## Current features

- Field-size selection: small, medium, or large.
- Random starting layout with a 30% chance of each cell being alive.
- Conway's rules applied approximately once per second.
- Console display using `#` for living cells and `.` for dead cells.
- Input, board-size, and coordinate validation.

Cells outside the board are treated as dead. Each generation is calculated on a separate board so updates happen consistently.

## Structure

- **Board** — stores cell states and validates access.
- **Game** — counts neighbours and calculates generations.
- **BoardFactory** — creates random starting boards.
- **ConsoleRenderer** — displays the field.
- **ConsoleApplication** — handles the menu and simulation loop.
- **Program** — creates the objects and starts the application.

## Run

Requires the **.NET 10 SDK**.

From the repository root:

```powershell
dotnet run --project src/GameOfLife.ConsoleApp
```

Choose `1`, `2`, or `3` to start a game. Choose `0` to exit the menu, or press `Ctrl+C` during the simulation.

## Next steps

- Verify the game rules with automated tests.
- Separate game logic into a class library.
- Add generation and living-cell counters.
- Implement saving, loading, and graceful stopping.
- Support multiple parallel games and combined statistics.
