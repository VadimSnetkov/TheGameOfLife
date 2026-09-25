namespace GameOfLife.ConsoleApp.Models
{
    public class Game
    {
        public Board CurrentBoard { get; private set; }

        public Game(Board initialBoard)
        {
            ArgumentNullException.ThrowIfNull(initialBoard);
            CurrentBoard = initialBoard;
        }

        public void CalculateNextGeneration()
        {
            var nextBoard = new Board(
                CurrentBoard.Rows,
                CurrentBoard.Columns);

            for (int row = 0; row < CurrentBoard.Rows; row++)
            {
                for (int column = 0; column < CurrentBoard.Columns; column++)
                {
                    int neighbours = CountLivingNeighbours(row, column);
                    bool isAlive = CurrentBoard.IsAlive(row, column);

                    bool willBeAlive = neighbours == 3 || (isAlive && neighbours == 2);

                    nextBoard.SetCell(row, column, willBeAlive);
                }
            }

            CurrentBoard = nextBoard;
        }
        //Calculates and replaces the board using Conway’s rules

        private int CountLivingNeighbours(int row, int column)
        {
            int count = 0;

            for (int rowOffset = -1; rowOffset <= 1; rowOffset++)
            {
                for (int columnOffset = -1; columnOffset <= 1; columnOffset++)
                {
                    //the cell itself is not its own neighbour
                    if (rowOffset == 0 && columnOffset == 0)
                    {
                        continue;
                    }

                    int neighbourRow = row + rowOffset;
                    int neighbourColumn = column + columnOffset;

                    //outside the board = dead.
                    if (neighbourRow < 0 ||
                        neighbourRow >= CurrentBoard.Rows ||
                        neighbourColumn < 0 ||
                        neighbourColumn >= CurrentBoard.Columns)
                    {
                        continue;
                    }

                    if (CurrentBoard.IsAlive(neighbourRow, neighbourColumn))
                    {
                        count++;
                    }
                }
            }

            return count;
        }
        //Counts living neighbouring cells within the board boundaries
    }
}
