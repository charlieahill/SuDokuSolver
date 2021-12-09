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
            OutputToScreen();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            board = board.Solve();
            OutputNumberOfCompletedSquares();
            OutputToScreen();

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
                {
                    MessageBox.Text = "WINNER!";
                    return;
                }

            MessageBox.Text = "FAILED";

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
            OptionsLabel.Text = $"Cell ({Cell.X}, {Cell.Y}): Options: {OptionsText}";
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
}
