using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuDokuSolver.Solver
{
    public static class Solver
    {
        /// <summary>
        /// Takes in a partially completed board and outputs the solved board as far as possible
        /// </summary>
        /// <param name="board">The partially completed board to be solved</param>
        /// <returns>The resolved board as far as possible</returns>
        public static int[,] Solve(this int[,] board)
        {
            SuDokuGameResult SimpleSolveResult = SimpleSolve(board, null);

            if (SimpleSolveResult != SuDokuGameResult.Open)
                return board;

            //Go through every single option in the options matrix, and see if any combination gives a win/if any combination can be ruled out
            List<IntAndResult>[,] MasterOptionsMatrix = new List<IntAndResult>[9, 9];
            if (board.TryAllOptions(MasterOptionsMatrix))
            {
                board = board.AddWinningResultToBoard(MasterOptionsMatrix);

                //we have a winner!!!
                SimpleSolve(board, null);
                return board; //if the trial comes back as a win, then retry
            }

            //then repeat the simple solve
            SimpleSolve(board, MasterOptionsMatrix);

            //and for now just return the board
            return board;
        }

        /// <summary>
        /// Applies simple solutions based on unique options to fit into cells
        /// </summary>
        /// <param name="board">The partially completed board about which to provide the simple solutions</param>
        /// <param name="gameResult">Returns a result from the game</param>
        /// <param name="MasterOptionsMatrix">Previously declared Mater Options Matrix (eliminates options that can only lead to loss)</param>
        /// <returns>The input board with the simple solutions appied.</returns>
        private static SuDokuGameResult SimpleSolve(int[,] board, List<IntAndResult>[,] MasterOptionsMatrix)
        {
            List<int>[,] OptionsMatrix;

            bool newUniqueOptionsGenerated;
            //Keep running through all of the different options, update the options matrix etc., until no new improvements can be found
            do
            {
                OptionsMatrix = board.GenerateOptionsMatrix(MasterOptionsMatrix);

                //Game should return lost - if, in the process of updating the Options matrix, and cells are left with zero options (check)
                if (OptionsMatrixContainsBlanks(board, OptionsMatrix))
                    return SuDokuGameResult.Loss;

                newUniqueOptionsGenerated = ApplyUniqueOptions(board, OptionsMatrix);

                if (newUniqueOptionsGenerated)
                    continue;

                if (CheckForSingleValuesInRows(board, OptionsMatrix))
                    newUniqueOptionsGenerated = true;

                if (newUniqueOptionsGenerated)
                    continue;

                if (CheckForSingleValuesInCols(board, OptionsMatrix))
                    newUniqueOptionsGenerated = true;

                if (newUniqueOptionsGenerated)
                    continue;

                if (CheckForSingleValuesInSquare(board, OptionsMatrix))
                    newUniqueOptionsGenerated = true;

            } while (newUniqueOptionsGenerated);

            if (CompletedSquares(board) == 81)
                return SuDokuGameResult.Win;

            return SuDokuGameResult.Open;
        }

        /// <summary>
        /// Generates all options for all squares on a sudoku board, based on the already completed numbers
        /// </summary>
        /// <param name="board">The partially completed board to generate the options matrix</param>
        /// <param name="MasterOptionsMatrix">Includes details of any options to be avoided</param>
        /// <returns>The list of options for each cell reference</returns>
        public static List<int>[,] GenerateOptionsMatrix(this int[,] board, List<IntAndResult>[,] MasterOptionsMatrix)
        {
            List<int>[,] Options = new List<int>[9, 9];

            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    Options[i, j] = new List<int>();

                    for (int k = 1; k < 10; k++)
                    {
                        if (board.IsNotInRow(k, i))
                            if (board.IsNotInColumn(k, j))
                                if (board.IsNotInSquare(k, i, j))
                                    //if value is not already set in the board
                                    if (board[i, j] == 0)
                                        //don't include a number that is known to give a loss
                                        if (MasterOptionsMatrix == null || MasterOptionsMatrix[i, j] == null || MasterOptionsMatrix[i, j].Where(x => x.Number == k).First().Result != SuDokuGameResult.Loss)
                                            Options[i, j].Add(k);
                    }
                }
            }

            return Options;
        }

        /// <summary>
        /// Determines if a number already exists in a SuDoku square
        /// </summary>
        /// <param name="board">The board to look up the questioned number</param>
        /// <param name="k">The number to check if it is already contained in the square</param>
        /// <param name="i">The column position of the square (0/1/2)</param>
        /// <param name="j">The row position of the square (0/1/2)</param>
        /// <returns>True if the square does not contain the target number. False otherwise.</returns>
        private static bool IsNotInSquare(this int[,] board, int k, int i, int j)
        {
            int startX = (int)Math.Floor(i / 3.0);
            int startY = (int)Math.Floor(j / 3.0);

            for (int x = startX * 3; x < (startX * 3) + 3; x++)
            {
                for (int y = startY * 3; y < (startY * 3) + 3; y++)
                {
                    if (board[x, y] == k)
                        return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Determines if a number already exists in a column
        /// </summary>
        /// <param name="board">The board to look up the questioned number</param>
        /// <param name="k">The number to check if it is already contained in the column</param>
        /// <param name="j">The column number in which to look for the questioned number</param>
        /// <returns>True if the column contains the target number. False otherwise.</returns>
        private static bool IsNotInColumn(this int[,] board, int k, int j)
        {
            for (int index = 0; index < 9; index++)
            {
                if (board[index, j] == k)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Determines if a number already exists in a row
        /// </summary>
        /// <param name="board">The board to look up the questioned number</param>
        /// <param name="k">The number to check if it is already contained in the row</param>
        /// <param name="i">The row number in which to look for the questioned number</param>
        /// <returns>True if the row contains the target number. False otherwise.</returns>
        private static bool IsNotInRow(this int[,] board, int k, int i)
        {
            for (int index = 0; index < 9; index++)
            {
                if (board[i, index] == k)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Looks through the options matrix, and for any cells that only have one option in the option matrix, set the board cell value to the unique value in the options matrix
        /// </summary>
        /// <param name="board">The board to apply the options to</param>
        /// <param name="Options">The options matrix to check for unique values</param>
        /// <returns>TRUE if any new unique options were applied, FALSE otherwise</returns>
        private static bool ApplyUniqueOptions(int[,] board, List<int>[,] Options)
        {
            for (int i = 0; i < 9; i++)
                for (int j = 0; j < 9; j++)
                    if (board[i, j] == 0)
                        if (Options[i, j].Count == 1)
                        {
                            board[i, j] = Options[i, j][0];
                            Debug.WriteLine($"({i},{j}) set to {board[i, j]}: Unique Option");
                            return true;
                        }

            return false;
        }

        /// <summary>
        /// Looks through every row in the options matrix to check for any unique options, and applies those unique row options to the board
        /// </summary>
        /// <param name="board">The SuDoku puzzle to apply the unique row values to</param>
        /// <param name="Options">The array of all options from which to apply the update</param>
        /// <returns>TRUE if any new values were applied from the rows, FALSE otherwise</returns>
        private static bool CheckForSingleValuesInRows(int[,] board, List<int>[,] Options)
        {
            //for every row
            for (int row = 0; row < 9; row++)
            {
                //for every number
                for (int number = 1; number < 10; number++)
                {
                    int NumberOfOccurencesOfNumber = 0;

                    //for each cell in the row
                    for (int colPosition = 0; colPosition < 9; colPosition++)
                    {
                        //count the number of occurences of the particular number in the row
                        if (Options[row, colPosition].Contains(number))
                            NumberOfOccurencesOfNumber++;
                    }

                    //if there is only one occurence of the number in the row
                    if (NumberOfOccurencesOfNumber == 1)
                    {
                        //find the occurence of that number
                        for (int colPosition = 0; colPosition < 9; colPosition++)
                        {
                            //check that the board is blank for this position
                            if (board[row, colPosition] == 0)
                                if (Options[row, colPosition].Contains(number))
                                {
                                    //Set that position to the number
                                    board[row, colPosition] = number;
                                    Debug.WriteLine($"({row},{colPosition}) set to {number}: Unique value in row");
                                    //if values have changed, set the update value to TRUE. Allow it to continue for further updates
                                    return true;
                                }
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Looks through every column in the options matrix to check for any unique options, and applies those unique column options to the board
        /// </summary>
        /// <param name="board">The SuDoku puzzle to apply the unique column values to</param>
        /// <param name="Options">The array of all options from which to apply the update</param>
        /// <returns>TRUE if any new values were applied from the columns, FALSE otherwise</returns>
        private static bool CheckForSingleValuesInCols(int[,] board, List<int>[,] Options)
        { 
            //for every column
            for (int col = 0; col < 9; col++)
            {
                //for every number
                for (int number = 1; number < 10; number++)
                {
                    int NumberOfOccurencesOfNumber = 0;

                    //for each row in the column
                    for (int rowPosition = 0; rowPosition < 9; rowPosition++)
                    {
                        //count the number of occurences of the particular number in the column
                        if (Options[rowPosition, col].Contains(number))
                            NumberOfOccurencesOfNumber++;
                    }

                    //if there is only one occurence of the number in the column
                    if (NumberOfOccurencesOfNumber == 1)
                    {
                        //find the occurence of that number
                        for (int rowPosition = 0; rowPosition < 9; rowPosition++)
                        {
                            //check that the board is blank for this position
                            if (board[rowPosition, col] == 0)
                                if (Options[rowPosition, col].Contains(number))
                                {
                                    //Set that position to the number
                                    board[rowPosition, col] = number;
                                    Debug.WriteLine($"({rowPosition},{col}) set to {number}: Unique value in column");
                                    //if values have changed, set the update value to TRUE. Allow it to continue for further updates
                                    return true;
                                }
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Looks through every square in the options matrix to check for any unique options, and applies those unique options to the board
        /// </summary>
        /// <param name="board">The SuDoku puzzle to apply the unique square values to</param>
        /// <param name="Options">The array of all options from which to apply the update</param>
        /// <returns>TRUE if any new values were applied from the squares, FALSE otherwise</returns>
        private static bool CheckForSingleValuesInSquare(int[,] board, List<int>[,] Options)
        {
            //Go through each square in turn
            for (int square = 0; square < 9; square++)
            {
                //Calculate the start and stop co-ordinates of each square
                int startX = (int)Math.Floor(square / 3.0);
                int startY = (int)(square % 3.0);

                //Loop through each number
                for (int num = 1; num < 10; num++)
                {
                    int NumberOfOccurencesOfNumber = 0;

                    //Loop through every cell within the square
                    for (int col = 3 * startX; col < (3 * startX) + 3; col++)
                    {
                        for (int row = 3 * startY; row < (3 * startY) + 3; row++)
                        {
                            //Count the number of occurences of the given number
                            if (Options[row, col].Contains(num))
                                NumberOfOccurencesOfNumber++;
                        }
                    }

                    //If the given number has one occurence
                    if (NumberOfOccurencesOfNumber == 1)
                    {
                        //Find the location of that number
                        for (int col = 3 * startX; col < (3 * startX) + 3; col++)
                        {
                            for (int row = 3 * startY; row < (3 * startY) + 3; row++)
                            {
                                if (board[row, col] == 0)
                                    //And set that number in the SuDoku game
                                    if (Options[row, col].Contains(num))
                                    {
                                        board[row, col] = num;
                                        Debug.WriteLine($"({row},{col}) set to {num}: Unique value in square");
                                        return true;
                                    }
                            }
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Returns the number of completed squares in a SuDoku board
        /// </summary>
        /// <param name="board">The SuDoku board to evaluate</param>
        /// <returns>The number of completed squares in that board</returns>
        private static int CompletedSquares(int[,] board)
        {
            int CompletedSquares = 0;

            for (int i = 0; i < 9; i++)
                for (int j = 0; j < 9; j++)
                    if (board[i, j] != 0)
                        CompletedSquares++;

            return CompletedSquares;
        }

        /// <summary>
        /// Looks at the options matrix and SuDoku board to determine if the game is still winnable
        /// </summary>
        /// <param name="board">The SuDoku board to analyse</param>
        /// <param name="optionsMatrix">The options matrix for the board</param>
        /// <returns>TRUE if the options matrix and board combined contain blanks (game is unwinnable), FALSE otherwise</returns>
        private static bool OptionsMatrixContainsBlanks(int[,] board, List<int>[,] optionsMatrix)
        {
            for (int row = 0; row < 9; row++)
                for (int col = 0; col < 9; col++)
                    if (optionsMatrix[row, col].Count == 0 && board[row, col] == 0)
                        return true;

            return false;
        }

        /// <summary>
        /// Test every single option to see if any option delivers either a game win, or a game loss.
        /// Wins automatically return the winning board
        /// Losses are ruled out of being an option in the future
        /// </summary>
        /// <param name="board">The board to analyse</param>
        /// <param name="masterOptionsMatrix">The matrix that stores if a board is a game win or a game loss</param>
        /// <returns>TRUE if the game is won. FALSE otherwise</returns>
        private static bool TryAllOptions(this int[,] board, List<IntAndResult>[,] masterOptionsMatrix)
        {
            int[,] InitialBoard = (int[,])board.Clone();
            masterOptionsMatrix.Init();

            //Generate an options matrix
            List<int>[,] OptionsMatrix;
            OptionsMatrix = board.GenerateOptionsMatrix(null);
            List<CellOptionModel> CellOptions = OptionsMatrix.ConvertToCellOptionModel();//build up a list of trial results

            foreach (CellOptionModel cellOption in CellOptions)
            {
                //reset the board to the original board
                board = (int[,])InitialBoard.Clone();
                //Reset the options matrix each time
                OptionsMatrix = board.GenerateOptionsMatrix(null);

                board[cellOption.XCoordinate, cellOption.YCoordinate] = cellOption.Value;
                Debug.WriteLine($"Trying {cellOption.Value} in space ({cellOption.XCoordinate}, {cellOption.YCoordinate})");

                SuDokuGameResult SimpleSolveResult = SimpleSolve(board, null);
                masterOptionsMatrix[cellOption.XCoordinate, cellOption.YCoordinate].Add(new IntAndResult(cellOption.Value, SimpleSolveResult));
                Debug.WriteLine($"Tried {cellOption.Value} in space ({cellOption.XCoordinate}, {cellOption.YCoordinate}) with result {SimpleSolveResult}");

                if (SimpleSolveResult == SuDokuGameResult.Win)
                    return true;
            }

            //Always make sure board is reset
            board = (int[,])InitialBoard.Clone();
            return false;
        }

        /// <summary>
        /// Loops through every cell in the masters options matrix and initialises it with a new List of IntAndResult
        /// </summary>
        /// <param name="masterOptionsMatrix">The master options matrix to initialise</param>
        private static void Init(this List<IntAndResult>[,] masterOptionsMatrix)
        {
            for (int i = 0; i < masterOptionsMatrix.GetLength(0); i++)
                for (int j = 0; j < masterOptionsMatrix.GetLength(1); j++)
                    masterOptionsMatrix[i, j] = new List<IntAndResult>();
        }

        /// <summary>
        /// Converts all options in the options matrix into a 1D list of values to try
        /// </summary>
        /// <param name="optionsMatrix">The options matrix to convert</param>
        /// <returns>A 1D list of values to try in the SUDoku grid</returns>
        private static List<CellOptionModel> ConvertToCellOptionModel(this List<int>[,] optionsMatrix)
        {
            List<CellOptionModel> options = new List<CellOptionModel>();

            for (int i = 0; i < optionsMatrix.GetLength(0); i++)
                for (int j = 0; j < optionsMatrix.GetLength(1); j++)
                    foreach (int value in optionsMatrix[i, j])
                        options.Add(new CellOptionModel(i, j, value));

            return options;
        }

        /// <summary>
        /// Applies a winning result as a fixed result to the board
        /// </summary>
        /// <param name="board">The SuDoku board to have the results added to</param>
        /// <param name="masterOptionsMatrix">The list of results to apply to the SuDoku game</param>
        /// <returns>The SuDoku board with the winning result by simple solve inserted</returns>
        private static int[,] AddWinningResultToBoard(this int[,] board, List<IntAndResult>[,] masterOptionsMatrix)
        {
            for (int i = 0; i < board.GetLength(0); i++)
                for (int j = 0; j < board.GetLength(1); j++)
                    foreach (IntAndResult option in masterOptionsMatrix[i,j])
                        if (option.Result == SuDokuGameResult.Win)
                            board[i, j] = option.Number;

            return board;
        }
    }
}
