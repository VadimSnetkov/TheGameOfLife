namespace GameOfLife.ConsoleApp.Models
{
    public class BoardFactory
    {
        private const double InitialAliveProbability = 0.3;

        public Board CreateRandom(int rows, int columns)
        {
            var board = new Board(rows, columns);

            for (int row = 0; row < board.Rows; row++)
            {
                for (int column = 0; column < board.Columns; column++)
                {
                    bool isAlive = Random.Shared.NextDouble() < InitialAliveProbability;
                    board.SetCell(row, column, isAlive);
                }
            }

            return board;
        }
        // Creates a board with randomly populated cells
    }
}
