using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;

///<ToDo>
///
/// Test for more unsolvable puzzles
/// Grab puzzle from screen and autopopulate boxes
/// 
///</ToDo>


/// <SUDOKU_RULES>
/// 
///  -----   Last Free Cell  -----
///  
///     If there is only 1 free cell in a section, there can only be one number that can go there
/// 
///  ****-----   Last Remaining Cell  -----****
///  
///     Numbers cannot be repeated in a given section. If there is only one square to place a number, the number must go there 
///     
///  ****-----   Last Possible Number  -----****
///  
///     If only 1 number can go in a cell, then the number must be placed there
/// 
///  ****-----   Obvious Singles  -----****
/// 
/// 
/// 
///  ****-----   Obvious Pairs  -----****
/// 
///     The point is that you should find 2 cells with the same pairs of Possibilities within a section.
///     This means that these pairs of Possibilities cannot be used in other cells within this section.
///     Hence, we remove them from other squares' possibilities. 
/// 
/// 
/// ****-----   Obvious Triples  -----****
/// 
///     This means that these cells have number 1, 5 & 8 in them but we don't know yet where each number is exactly.
///     What we know though, is that 1, 5 & 8 can't be in other cells of this block. So, we can remove them from the notes.
/// 
///  ****-----   Hidden Singles  -----****
/// 
///     {SEE LAST REMAINING CELL}
/// 
///  ****-----   Hidden Pairs  -----****
/// 
///     If you can find two cells within a row, column, or 3x3 block where two possibilities appear nowhere outside these cells,
///     these two possibilities must be placed in the two cells. All other Notes can be eliminated from these two cells.
/// 
///  ****-----   Hidden Triples  -----****
/// 
///     "Hidden triples" technique is very similar to "Hidden pairs" and works on the same concept.
///     "Hidden triples" applies when three cells in a row, column, or 3x3 block contain the same three Notes. These three cells also contain other candidates, which may be removed from them.
///     It will be easier to understand this technique if you look at the example.
///     For Example, there are only three cells, which contain repeated numbers: 5, 6 and 7. This means each of these numbers must occupy one of these cells. And any other numbers cannot be found here.
///     If so, 5,6 and 7 cannot be presented in any other cell of this 3x3 block as well.
/// 
///  ****-----   Pointing Pairs  -----****
/// "Pointing pairs" applies when a Note is present twice in a block and this Note also belongs to the same row or column. This means that the Note must be the solution for one of the two cells in the block. So, you can eliminate this Note from any other cells in the row or column.
/// To understand "Pointing pairs" better, let's take a look at the example.
/// Let's look at the block at the top left corner. All the cells that might contain number 4 are located in one column. As number 4 should appear in this block at least once, one of the highlighted cells will surely contain 4.
/// 
/// 
///  ****-----   Pointing Triples  -----****
/// 
/// "Pointing triples" technique is very similar to "Pointing pairs". It applies if a Note is present in only three cells of a 3x3 block and also belongs to the same row or column. This means that the Note must be a solution for one of these three cells in the block. So, obviously it can't be a solution of any other cell in the row or column and can be eliminated from them.
/// For example:
/// Let's take a look at the bottom right corner. 
/// In this block all the cells that might contain number 1 are located in one row. As number 1 must appear in the bottom right block at least once, one of the highlighted cells will surely contain 1.
/// After this conclusion all other possible numbers 1 can be safely deleted from the Notes of this row to avoid confusion. 
/// Remember that you can do the same trick for blocks, rows, and columns.
/// 
///  ****-----   X Wing  -----****
/// 
/// 
/// 
///  ****-----   Y Wing  -----****
/// 
/// 
/// 
///  ****-----   Swordfish  -----****
/// 
/// 
/// 
/// </SUDOKU_RULES>

namespace Sudoku_Solver
{


    public partial class MainWindow : Window
    {
        //magic number reduction
        public const int DefaultFontSize = 32, PossibilityFontSize = 8, //font sizes
        maxIterations = 1000; // the maximum number of iterations that the program will loop for - prevents infinite loops


        Square[,] grid = new Square[9, 9];
        Section[] Sections = new Section[27];
        public bool updated;


        public MainWindow()
        {
            InitializeComponent();

            for (int i = 0; i < Sections.Count(); i++)
            {
                Sections[i] = new Section(new List<Square>());
            }

            InitializeGrid();
        }


        //when the SOLVE button is pressed
        public void Button_Click(object sender, RoutedEventArgs e)
        {
            int iterations = 0;

            //gather initial solved squares
            foreach (Square square in grid)
            {
                //grab the contents of the square's text box
                string contents = square.tb.Text;

                //if there is more than one character in the text box, do not read data from it
                if (!contents.Equals("") && contents.Length == 1)
                {
                    square.set = true;
                    square.final = int.Parse(square.tb.Text);
                    square.possibilities.Clear();
                    square.TrimPossibilities(grid);
                    square.tb.FontSize = 32;
                }

            }
        recheck:
            updated = false;
            foreach (Section s in Sections)
            {
                updated |= s.ScanForPairs();
                updated |= s.ScanForOnlySection(Sections);
                updated |= s.CheckForHiddenPairs();
                updated |= s.ScanForOnlyPlace();
                updated |= s.CheckForPointingPairs(Sections);
                updated |= s.ScanForObviousTriples();
                
            }

            updated |= ScanForXWing();
            updated |= ScanForYWing();

            //fill squares and trim the squares they effect
            foreach (Square square in grid)
            {
                if (square.possibilities.Count == 0 && !square.set)
                {
                    // MessageBox.Show("A square with no possible solutions has been detected.");
                    square.tb.Background = Brushes.Red;
                }
                if (square.possibilities.Count == 1 && !square.set)
                {
                    square.set = true;
                    square.final = square.possibilities[0];
                    square.tb.Text = square.possibilities[0].ToString();
                    square.tb.FontSize= DefaultFontSize;
                    square.TrimPossibilities(grid);
                    Debug.WriteLine("Square Solved");
                }

            }

            iterations++;
            if (updated && iterations < maxIterations)
            {
                goto recheck;
            }
            else
            {
               displayPossiblitites(grid);
               MessageBox.Show("unable to continue");
            }

        }

        //button to clear possibilitites
        private void ClearPossibiliites_Click(object sender, RoutedEventArgs e)
        {
            foreach (Square square in grid)
            {
                if (!square.set)
                {
                    square.tb.Text = "";
                    square.tb.FontSize = 32;
                }
            }

        }

        //debug function to show which sections a text box is in
        private void DisplaySections_Click(object sender, RoutedEventArgs e)
        {
            foreach (Square square in grid)
            {
                square.tb.FontSize = 12;
                string temp = "";
                foreach (int i in square.sections)
                {
                    temp += i.ToString() + ",";
                }
                square.tb.Text = temp;
            }
        }

        public bool ScanForXWing()
        {
            bool updated=false;
            for (int number = 1; number <= 9; number++)
            {
                Dictionary<int, List<int>> rowMap = new();

                // Track columns where number appears per row
                for (int row = 0; row < 9; row++)
                {
                    var cols = new List<int>();
                    for (int col = 0; col < 9; col++)
                    {
                        if (!grid[row, col].set && grid[row, col].possibilities.Contains(number))
                        {
                            cols.Add(col);
                        }
                    }

                    if (cols.Count == 2)
                    {
                        rowMap[row] = cols;
                    }
                }

                // Look for matching column pairs across two rows
                var rows = rowMap.Keys.ToList();
                for (int i = 0; i < rows.Count; i++)
                {
                    for (int j = i + 1; j < rows.Count; j++)
                    {
                        if (rowMap[rows[i]].SequenceEqual(rowMap[rows[j]]))
                        {
                            int col1 = rowMap[rows[i]][0];
                            int col2 = rowMap[rows[i]][1];

                            for (int row = 0; row < 9; row++)
                            {
                                if (row != rows[i] && row != rows[j])
                                {
                                    if (grid[row, col1].possibilities.Remove(number)) updated = true;
                                    if (grid[row, col2].possibilities.Remove(number)) updated = true;
                                }
                            }
                        }
                    }
                }
            }

            return updated;
        }

        bool ScanForYWing()
        {
            bool updated = false;

            // Find all bivalue squares
            var bivalueSquares = new List<Square>();
            for (int row = 0; row < 9; row++)
            {
                for (int col = 0; col < 9; col++)
                {
                    var square = grid[row, col];
                    if (!square.set && square.possibilities.Count == 2)
                    {
                        bivalueSquares.Add(square);
                    }
                }
            }

            foreach (var pivot in bivalueSquares)
            {
                var pivotCandidates = pivot.possibilities.ToList();
                int a = pivotCandidates[0];
                int b = pivotCandidates[1];

                var pivotPeers = GetPeers(grid, pivot);

                // Find Wing A: shares candidate 'a' and is bivalue with {a, c}
                foreach (var wingA in pivotPeers)
                {
                    if (!wingA.set && wingA.possibilities.Count == 2 &&
                        wingA.possibilities.Contains(a) && !wingA.possibilities.Contains(b))
                    {
                        int c = wingA.possibilities.First(x => x != a);

                        // Find Wing B: shares candidate 'b' and is bivalue with {b, c}
                        foreach (var wingB in pivotPeers)
                        {
                            if (wingB == wingA || wingB.set || wingB.possibilities.Count != 2)
                                continue;

                            if (wingB.possibilities.Contains(b) && wingB.possibilities.Contains(c) && !wingB.possibilities.Contains(a))
                            {
                                // WingA and WingB must see a common square (target)
                                var commonPeers = GetPeers(grid, wingA).Intersect(GetPeers(grid, wingB));

                                foreach (var target in commonPeers)
                                {
                                    if (!target.set && target.possibilities.Contains(c))
                                    {
                                        target.possibilities.Remove(c);
                                        updated = true;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return updated;
        }

        HashSet<Square> GetPeers(Square[,] grid, Square square)
        {
            var peers = new HashSet<Square>();

            for (int i = 0; i < 9; i++)
            {
                if (i != square.column) peers.Add(grid[square.row, i]);
                if (i != square.row) peers.Add(grid[i, square.column]);
            }

            int boxStartRow = (square.row / 3) * 3;
            int boxStartCol = (square.column / 3) * 3;

            for (int r = boxStartRow; r < boxStartRow + 3; r++)
            {
                for (int c = boxStartCol; c < boxStartCol + 3; c++)
                {
                    if (r != square.row || c != square.column)
                        peers.Add(grid[r, c]);
                }
            }

            return peers;
        }

        void displayPossiblitites(Square[,] grid)
        {
            foreach (Square s in grid)
            {
                if (!s.set)
                {
                    s.tb.FontSize = PossibilityFontSize;
                    string temp = "";


                    foreach (int i in s.possibilities)
                    {
                        temp += i.ToString() + ",";
                    }
                    s.tb.Text = temp;
                }
            }
        }

        //function that creates the sudoku grid in the UI
        void InitializeGrid()
        {
            for (int x = 0; x < 9; x++)
            {
                for (int y = 0; y < 9; y++)
                {
                    Square s = new Square(x, y);
                    TextBox t = s.tb;
                    t.FontSize = DefaultFontSize;

                    // Create a Border to wrap the TextBox
                    Border b = new Border();
                    b.Child = t;

                    // Determine thickness for sudoku-style borders
                    Thickness thickness = new Thickness(
                        left: y % 3 == 0 ? 3 : 1,
                        top: x % 3 == 0 ? 3 : 1,
                        right: y == 8 ? 3 : 1,
                        bottom: x == 8 ? 3 : 1
                    );
                    b.BorderThickness = thickness;
                    b.BorderBrush = Brushes.Black;

                    Grid.SetRow(b, x);
                    Grid.SetColumn(b, y);
                    tbGrid.Children.Add(b);

                    grid[x, y] = s;

                    int Ssection = s.section;
                    Sections[Ssection].Add(s);
                    Sections[9 + x].Add(s);
                    Sections[18 + y].Add(s);
                }
            }
        }

    }
}