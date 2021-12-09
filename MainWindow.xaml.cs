using SuDokuSolver.Solver;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SuDokuSolver
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        int[,] board = new int[9,9];
        List<int>[,] Options = new List<int>[9, 9];
        bool IsManualInput = true;
        public MainWindow()
        {
            InitializeComponent();
            UpdateOptions();
            FillSampleData();
        }

        private void FillSampleData()
        {
            /*board[0, 0] = 3;
            board[1, 0] = 2;
            board[3, 0] = 8;
            board[5, 0] = 7;
            board[7, 0] = 9;
            board[8, 0] = 6;
            board[4, 1] = 6;
            board[1, 2] = 1;
            board[3, 2] = 9;
            board[5, 2] = 2;
            board[7, 2] = 4;
            board[0, 3] = 8;
            board[2, 3] = 2;
            board[6, 3] = 5;
            board[8, 3] = 1;
            board[1, 4] = 3;
            board[7, 4] = 8;
            board[0, 5] = 1;
            board[2, 5] = 4;
            board[6, 5] = 9;
            board[8, 5] = 7;
            board[1, 6] = 6;
            board[3, 6] = 1;
            board[5, 6] = 4;
            board[7, 6] = 7;
            board[4, 7] = 5;
            board[0, 8] = 2;
            board[1, 8] = 4;
            board[3, 8] = 6;
            board[5, 8] = 9;
            board[7, 8] = 5;
            board[8, 8] = 3;*/

            /*board[0, 5] = 2;
            board[1, 5] = 4;
            board[1, 6] = 1;
            board[1, 7] = 8;
            board[2, 1] = 2;
            board[2, 4] = 8;
            board[2, 7] = 4;
            board[3, 2] = 9;
            board[3, 7] = 5;
            board[3, 8] = 3;
            board[4, 0] = 7;
            board[4, 2] = 5;
            board[4, 3] = 2;
            board[4, 6] = 6;
            board[5, 0] = 6;
            board[5, 1] = 1;
            board[5, 4] = 7;
            board[6, 4] = 2;
            board[6, 5] = 1;
            board[7, 0] = 4;
            board[7, 3] = 6;
            board[7, 6] = 9;
            board[8, 0] = 5;
            board[8, 1] = 9;
            board[8, 3] = 8;
            board[8, 4] = 4*/

            board[0, 0] = 4;
            board[0, 2] = 6;
            board[0, 8] = 3;
            board[1, 1] = 2;
            board[1, 4] = 6;
            board[2, 1] = 9;
            board[2, 6] = 4;
            board[3, 0] = 2;
            board[3, 1] = 3;
            board[3, 2] = 9;
            board[3, 5] = 8;
            board[3, 8] = 1;
            board[4, 1] = 4;
            board[5, 0] = 5;
            board[5, 1] = 6;
            board[5, 2] = 7;
            board[5, 4] = 9;
            board[5, 6] = 3;
            board[6, 1] = 5;
            board[6, 2] = 4;
            board[6, 3] = 9;
            board[6, 7] = 7;
            board[7, 0] = 9;
            board[7, 1] = 7;
            board[7, 2] = 2;
            board[7, 7] = 3;
            board[8, 0] = 6;
            board[8, 2] = 3;
            board[8, 4] = 7;
            board[8, 6] = 5;



            OutputToScreen();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //IsManualInput = false;
            //Solve();
            //IsManualInput = true;
            //OutputToScreen();

            board = board.Solve();
            OutputNumberOfCompletedSquares();
            OutputToScreen();

        }

        public bool Solve()
        {
            //1. Solve as far as possible with the simple solver


            bool newUniqueOptionsGenerated;
            do
            {
                newUniqueOptionsGenerated = ApplyUniqueOptions(board);

                if (CheckForSingleValuesInRows())
                    newUniqueOptionsGenerated = true;

                if (CheckForSingleValuesInCols())
                    newUniqueOptionsGenerated = true;

                if (CheckForSingleValuesInSquare())
                    newUniqueOptionsGenerated = true;

                if (!UpdateOptions())
                    return false;

            } while (newUniqueOptionsGenerated);

            if (CompletedSquares() != 81)
            {
                List<Point> TwoOptions = new List<Point>();

                for (int i = 0; i < 9; i++)
                    for (int j = 0; j < 9; j++)
                        if (Options[i, j].Count == 2)
                            TwoOptions.Add(new Point(i, j));

                AssessTwoOptions(TwoOptions);
            }

            OutputNumberOfCompletedSquares();

            return CompletedSquares() == 81;
        }

        private bool ApplyUniqueOptions(int[,] board)
        {
            bool newUniques = false;

            for (int i = 0; i < 9; i++)
                for (int j = 0; j < 9; j++)
                    if (board[i, j] == 0)
                        if (Options[i, j].Count == 1)
                        {
                            board[i, j] = Options[i, j][0];
                            Debug.WriteLine($"({i},{j}) set to {board[i, j]}: Unique Option");
                            newUniques = true;
                        }

            return newUniques;
        }

        private bool CheckForSingleValuesInRows()
        {
            bool newValues = false;

            for (int row = 0; row < 9; row++)
            {
                for (int number = 1; number < 10; number++)
                {
                    int NumberOfOccurencesOfNumber = 0;

                    for (int colPosition = 0; colPosition < 9; colPosition++)
                    {
                        if (Options[row, colPosition].Contains(number))
                            NumberOfOccurencesOfNumber++;
                    }

                    if (NumberOfOccurencesOfNumber == 1)
                    {
                        for (int colPosition = 0; colPosition < 9; colPosition++)
                        {
                            if (board[row, colPosition] == 0)
                                if (Options[row, colPosition].Contains(number))
                                {
                                    board[row, colPosition] = number;
                                    Debug.WriteLine($"({row},{colPosition}) set to {number}: Unique value in row");
                                    newValues = true;
                                }
                        }
                    }
                }
            }

            return newValues;
        }

        private bool CheckForSingleValuesInCols()
        {
            bool newValues = false;

            for (int col = 0; col < 9; col++)
            {
                for (int number = 1; number < 10; number++)
                {
                    int NumberOfOccurencesOfNumber = 0;

                    for (int rowPosition = 0; rowPosition < 9; rowPosition++)
                    {
                        if (Options[rowPosition, col].Contains(number))
                            NumberOfOccurencesOfNumber++;
                    }

                    if (NumberOfOccurencesOfNumber == 1)
                    {
                        for (int rowPosition = 0; rowPosition < 9; rowPosition++)
                        {
                            if (board[rowPosition, col] == 0)
                                if (Options[rowPosition, col].Contains(number))
                                {
                                    board[rowPosition, col] = number;
                                    Debug.WriteLine($"({rowPosition},{col}) set to {number}: Unique value in column");
                                    newValues = true;
                                }
                        }
                    }
                }
            }

            return newValues;
        }

        private bool CheckForSingleValuesInSquare()
        {
            bool newValues = false;

            for (int square = 0; square < 9; square++)
            {
                int startX = (int)Math.Floor(square / 3.0);
                int startY = (int)(square % 3.0);

                for (int num = 1; num < 10; num++)
                {
                    int NumberOfOccurencesOfNumber = 0;

                    for (int col = 3 * startX; col < (3 * startX) + 3; col++)
                    {
                        for (int row = 3 * startY; row < (3 * startY) + 3; row++)
                        {
                            //Debug.WriteLine($"Cell check ({col}, {row}) for number {num}. Square {square}");
                            if (Options[row, col].Contains(num))
                                NumberOfOccurencesOfNumber++;
                        }
                    }

                    if (NumberOfOccurencesOfNumber == 1)
                    {
                        for (int col = 3 * startX; col < (3 * startX) + 3; col++)
                        {
                            for (int row = 3 * startY; row < (3 * startY) + 3; row++)
                            {
                                if (board[row, col] == 0)
                                    if (Options[row, col].Contains(num))
                                    {
                                        board[row, col] = num;
                                        Debug.WriteLine($"({row},{col}) set to {num}: Unique value in square");
                                        newValues = true;
                                    }
                            }
                        }
                    }
                }
            }

            for (int col = 0; col < 9; col++)
            {
                for (int number = 1; number < 10; number++)
                {
                    int NumberOfOccurencesOfNumber = 0;

                    for (int rowPosition = 0; rowPosition < 9; rowPosition++)
                    {
                        if (Options[rowPosition, col].Contains(number))
                            NumberOfOccurencesOfNumber++;
                    }

                    if (NumberOfOccurencesOfNumber == 1)
                    {
                        for (int rowPosition = 0; rowPosition < 9; rowPosition++)
                        {
                            if (board[rowPosition, col] == 0)
                                if (Options[rowPosition, col].Contains(number))
                                {
                                    board[rowPosition, col] = number;
                                    newValues = true;
                                }
                        }
                    }
                }
            }

            return newValues;
        }

        private bool AssessTwoOptions(List<Point> twoOptions)
        {
            int[,] OriginalBoard = (int[,])board.Clone();

            foreach (Point point in twoOptions)
            {
                board = (int[,])OriginalBoard.Clone();
                UpdateOptions();

                foreach (int option in Options[(int)point.X, (int)point.Y])
                {
                    board[(int)point.X, (int)point.Y] = option;

                    if (Solve())
                    {
                        OutputToScreen();
                        SuDokuBorder.BorderBrush = Brushes.Black;
                        return true;

                        //Debug.WriteLine($"({point.X}, {point.Y}) cannot be {option}. Game is unplayable.");
                        //Debug.WriteLine($"({point.X}, {point.Y}) Options are {option1} and {option2}");
                        //todo: When I run this I am getting the message "Completed Squares 81"
                    }
                    else
                    {
                        Debug.WriteLine($"({point.X}, {point.Y}) cannot be {option}. Game is unplayable.");
                    }
                }
            }

            board = (int[,])OriginalBoard.Clone();
            UpdateOptions();

            OutputToScreen();
            SuDokuBorder.BorderBrush = Brushes.Black;
            return true;
        }

        private void OutputNumberOfCompletedSquares()
        {
            Debug.WriteLine($"Completed squares {CompletedSquares()}");
        }

        private int CompletedSquares()
        {
            int CompletedSquares = 0;

            for (int i = 0; i < 9; i++)
                for (int j = 0; j < 9; j++)
                    if (board[i, j] != 0)
                        CompletedSquares++;

            return CompletedSquares;
        }

        private void OutputToScreen()
        {
            int index = 0;

            foreach (TextBox cell in SuDokuGrid.Children)
            {
                if (board[(int)Math.Floor(index / 9.0), index % 9] == 0)
                    cell.Text = "";
                else
                {
                    cell.Text = board[(int)Math.Floor(index / 9.0), index % 9].ToString();
                }

                index++;
            }

            if (CompletedSquares() == 81)
                CheckBoardIsCorrect();
            else
                MessageBox.Text = "";
        }

        private void CheckBoardIsCorrect()
        {
            if (AllColumnsAreCorrect())
                if (AllRowsAreCorrect())
                    if (AllBoxesAreCorrect())
                    {
                        MessageBox.Text = "WINNER!";
                        return;
                    }

            MessageBox.Text = "FAILED";

        }

        private bool AllBoxesAreCorrect()
        {
            return true; //todo
        }

        private bool AllRowsAreCorrect()
        {
            int occurencesOfNumber;

            for (int row = 0; row < 9; row++)
            {
                for (int number = 1; number < 10; number++)
                {
                    occurencesOfNumber = 0;
                    for(int col = 0; col < 9; col++)
                    {
                        if (board[row, col] == number)
                            occurencesOfNumber++;
                    }

                    if (occurencesOfNumber != 1)
                    {
                        Debug.WriteLine($"{occurencesOfNumber}x {number} in row {row}");
                        return false;
                    }
                }
            }

            return true;
        }

        private bool AllColumnsAreCorrect()
        {
            int occurencesOfNumber;

            for (int col = 0; col < 9; col++)
            {
                for (int number = 1; number < 10; number++)
                {
                    occurencesOfNumber = 0;
                    for (int row = 0; row < 9; row++)
                    {
                        if (board[row, col] == number)
                            occurencesOfNumber++;
                    }

                    if (occurencesOfNumber != 1)
                    {
                        Debug.WriteLine($"{occurencesOfNumber}x {number} in col {col}");
                        return false;
                    }
                }
            }

            return true;
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!IsManualInput)
                return;

            Point Cell = GetCellPosition((TextBox)sender);

            if (int.TryParse(((TextBox)sender).Text, out int value))
                board[(int)Cell.X, (int)Cell.Y] = value;
            else
                board[(int)Cell.X, (int)Cell.Y] = 0;

            UpdateOptions();
        }

        private bool UpdateOptions()
        {
            SuDokuBorder.BorderBrush = Brushes.Black;
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    Options[i, j] = new List<int>();

                    if(board[i,j] != 0)
                    {
                        Options[i, j].Add(board[i, j]);
                        continue;
                    }

                    for (int k = 1; k < 10; k++)
                    {
                        if (IsNotInRow(k, i))
                            if (IsNotInColumn(k, j))
                                if (IsNotInSquare(k, i, j))
                                    Options[i, j].Add(k);
                    }

                    if(Options[i,j].Count == 0)
                    {
                        //game is unwinnable
                        SuDokuBorder.BorderBrush = Brushes.Red;
                        return false;
                    }
                }
            }

            return true;
        }

        private bool IsNotInSquare(int k, int i, int j)
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

        private bool IsNotInColumn(int k, int j)
        {
            for (int index = 0; index < 9; index++)
            {
                if (board[index, j] == k)
                    return false;
            }

            return true;
        }

        private bool IsNotInRow(int k, int i)
        {
            for (int index = 0; index < 9; index++)
            {
                if (board[i, index] == k)
                    return false;
            }

            return true;
        }


        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            Point Cell = GetCellPosition((TextBox)sender);
            string OptionsText = "";
            foreach (int value in Options[(int)Cell.X, (int)Cell.Y])
            {
                OptionsText += value + ", ";
            }
            Debug.WriteLine($"Cell ({Cell.X}, {Cell.Y}): Options: {OptionsText}");
        }

        private Point GetCellPosition(TextBox input)
        {
            string CellPosition = input.Tag.ToString();
            string[] Cells = CellPosition.Split(" ", StringSplitOptions.RemoveEmptyEntries);
            return new Point(int.Parse(Cells[0]), int.Parse(Cells[1]));
        }

        private void ButtonClear_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < 9; i++)
                for (int j = 0; j < 9; j++)
                    board[i, j] = 0;

            OutputToScreen();
        }

        private void ButtonOutput_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < 9; i++)
                for (int j = 0; j < 9; j++)
                    if (board[i, j] != 0)
                        sb.AppendLine($"board[{i}, {j}] = {board[i,j]};");

            Clipboard.SetText(sb.ToString());
        }
    }

    //todo: Implement ability to move back before solver
    //todo: Solve the unsolveable SuDoku... what ifs?
    //todo: For the what ifs, implement the idea of an unsolveable puzzle. If any option makes the puzzle unsolveable, then it is not a good idea...
    //todo: if reverting to clone, update board...
    //todo: Fix issues
}
