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
///     Add bold lines between boxes
///     sanitize input
///     implement more solve functions
///     ensure that when we have a copied pair in boxes that they are being removed appropriately
///     
///     
/// 
///</ToDo>


//hidden pairs

///




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
class Square
{
    public TextBox tb;
    public bool set;
    public List<int> possibilities = new List<int>{ 1,2,3,4,5,6,7,8,9};
    public int row, column,section, final;
    public int[] sections;
    public Square(int x, int y)
    {
        row = x;
        column = y;
        //assign appropriate section
        
        {
            if (x <= 2)
            {
                if (y <= 2)
                {
                    section = 0;
                }
                else if (y <= 5)
                {
                    section = 1;
                }
                else if (y <= 8)
                {
                    section = 2;
                }
            }
            else if (x <= 5)
            {
                if (y <= 2)
                {
                    section = 3;
                }
                else if (y <= 5)
                {
                    section = 4;
                }
                else if (y <= 8)
                {
                    section = 5;
                }
            }
            else if (x <= 8)
            {
                if (y <= 2)
                {
                    section = 6;
                }
                else if (y <= 5)
                {
                    section = 7;
                }
                else if (y <= 8)
                {
                    section = 8;
                }

            }
        }     
        tb = new TextBox();
        tb.Text = "";
        tb.Width = 50;
        tb.Height = 50;
        tb.IsEnabled = true;
        tb.FontSize = 40;
        tb.TextAlignment = TextAlignment.Center;
        tb.MaxLength = 1;
        sections = new int[]{section, x + 9, y + 18};

    }

    public void TrimPossibilities(Square[,] grid)
    {
        foreach (Square s in grid)
        {
            if (s == this) continue;
            if (!s.set && (s.section == this.section || s.row == this.row || s.column == this.column))
            {
                s.possibilities.Remove(this.final);
            }
        }
    }

}

class Section
{
    public List<Square> squares = new List<Square>();
    

    public Section(List<Square> squa)
    {
        this.squares = squa;
    }
    //add a square to the section
    public void Add(Square square) 
    {
        squares.Add(square);
    }
    //scans for obvious pairs and updates the board accordingly
    public bool ScanForPairs(bool updated)
    {
        foreach(Square s in this.squares) //for each square in this section
        {
            if (!s.set && s.possibilities.Count ==2) //if only 2 things can go here
            {
                foreach(Square q in this.squares) //look at the other squares
                {
                    if(!q.set && s!=q && s.possibilities.SequenceEqual(q.possibilities)) //if they both have the same possible solutions
                    {
                        foreach(Square u in this.squares)
                        {
                            if(u!=s && u!=q) 
                            {
                                foreach (int i in s.possibilities)
                                {
                                    u.possibilities.Remove(i); //remove the possible numbers from the solutions list
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


    //scan each section to see if there is only 1 place a number can go
    public bool ScanForOnlyPlace(bool updated)
    {
        for (int i = 1; i < 10; i++)
        {
            int count = 0;
            foreach (Square s in this.squares)
            {
                if (s.possibilities.Contains(i) && !s.set)
                {
                    count++;
                    
                }
            }
            if(count == 1) // if this is the only place we can put that number in the section
            {
                foreach (Square s in this.squares)
                {
                    if (s.possibilities.Contains(i))
                    {                        
                       s.possibilities.Clear();
                       s.possibilities.Add(i);
                        updated = true;
                    }

                }
            }  
            
        }

        return updated;
    }

    
    public bool ScanForOnlySection(Section[] sections, bool updated)
    {
        //if a number has to go on one line within a square, trim possibilities from the square excluding the line the number has to go in
        //or if a number has to go within a square on a line, trim possibilities from the line

        //for each possibility
        for (int i = 1; i < 10; i++)
        {
            int otherSection1 = -1;
            int otherSection2 = -1;
            int commonSection = -1;


            bool set = false;
            bool broken = false;
            //check for squares that have it
            foreach (Square s in this.squares)
            {
             
                if (!s.set && s.possibilities.Contains(i))
                {
                    //store the sections that are not this one that the square is contained in
                    if (!set)
                    {
                        foreach (int x in s.sections)
                        {
                            if (sections[x] != this)
                            {
                                if (otherSection1 == -1)
                                {
                                    otherSection1 = x;
                                }
                                else if (otherSection2 == -1)
                                {
                                    otherSection2 = x;
                                }
                            }

                        }
                        set = true;
                    }
                    else if(commonSection == -1)
                    {
                        if(!s.sections.Contains<int>(otherSection1) || !s.sections.Contains<int>(otherSection2))
                            {
                            //if a square is found that has neither of these as a section, break the loop as this number can go elsewhere
                            broken = true;
                            break;
                        }
                        else
                            {
                             if(s.sections.Contains<int>(otherSection1))
                            {
                                commonSection = otherSection1;
                            }
                            else
                            {
                                commonSection = otherSection2;
                            }
                            }
                    }
                    else
                    {
                        if(!s.sections.Contains<int>(commonSection)) 
                        {
                            broken = true;
                            break;
                        }
                    }
                }
            
            }
            //if we iterare through all the squares and don't break the loop
            if(!broken&& commonSection!=-1)
            {
                //we must have found the case we are looking for
                foreach(Square s in this.squares) 
                {
                    //trim the appropriate squares
                    if (!sections[commonSection].squares.Contains<Square>(s))
                    {
                        s.possibilities.Remove(i);
                        updated = true;
                    }
                }
               
            }

        }

        return updated;
    }

    public bool CheckForHiddenPairs(bool updated)
    {
        // Map from candidate number to list of squares where it appears
        Dictionary<int, List<Square>> positions = new Dictionary<int, List<Square>>();

        for (int i = 1; i <= 9; i++)
        {
            positions[i] = this.squares
                .Where(s => !s.set && s.possibilities.Contains(i))
                .ToList();
        }

        // Check all pairs of numbers
        for (int i = 1; i <= 8; i++)
        {
            for (int j = i + 1; j <= 9; j++)
            {
                var iSquares = positions[i];
                var jSquares = positions[j];

                // Both must appear in exactly 2 squares
                if (iSquares.Count == 2 && jSquares.Count == 2)
                {
                    // Check if they appear in exactly the same squares
                    if (iSquares[0] == jSquares[0] && iSquares[1] == jSquares[1])
                    {
                        foreach (var square in iSquares)
                        {
                            // Only trim if the square has other possibilities
                            var allowed = new List<int> { i, j };
                            if (!square.possibilities.SequenceEqual(allowed))
                            {
                                square.possibilities = allowed;
                                updated = true;
                            }
                        }
                    }
                }
            }
        }

        return updated;
    }

    public bool CheckForPointingPairs(Section[] sections, bool updated)
    {
        for (int number = 1; number <= 9; number++)
        {
            var candidates = new List<Square>();

            // Find all squares in this box that could hold 'number'
            foreach (Square s in squares)
            {
                if (!s.set && s.possibilities.Contains(number))
                {
                    candidates.Add(s);
                }
            }

            if (candidates.Count < 2) continue;

            bool sameRow = candidates.All(s => s.row == candidates[0].row);
            bool sameCol = candidates.All(s => s.column == candidates[0].column);

            if (sameRow)
            {
                int targetRow = candidates[0].row;
                // Row section is index 9 + row
                foreach (Square s in sections[9 + targetRow].squares)
                {
                    if (!this.squares.Contains(s) && !s.set && s.possibilities.Contains(number))
                    {
                        s.possibilities.Remove(number);
                        updated = true;
                    }
                }
            }
            else if (sameCol)
            {
                int targetCol = candidates[0].column;
                // Column section is index 18 + column
                foreach (Square s in sections[18 + targetCol].squares)
                {
                    if (!this.squares.Contains(s) && !s.set && s.possibilities.Contains(number))
                    {
                        s.possibilities.Remove(number);
                        updated = true;
                    }
                }
            }
        }

        return updated;
    }

}

namespace Sudoku_Solver
{
    public partial class MainWindow : Window
    {
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
            
           //for each space on the board
            for (int x = 0; x<9; x++)
            {
                for (int y = 0; y<9; y++)
                {
                    //create a square and create a text box to represent it
                    Square s = new Square(x, y);
                    TextBox t = s.tb;
                    t.FontSize = 32;
                    
                    //place the text box in the window
                    Grid.SetRow(t, x);
                    Grid.SetColumn(t, y);
                    tbGrid.Children.Add(t);
                    grid[x, y] = s;

                    //add the square in to the appropriate section
                    int Ssection = s.section;
                    Sections[Ssection].Add(s);
                    Sections[9+x].Add(s);
                    Sections[18+y].Add(s);

                }
            }
        }

       //when the SOLVE button is pressed
        public void Button_Click(object sender, RoutedEventArgs e)
        {
            int iterations = 0, maxIterations = 1000;

            //gather initial solved squares
            foreach (Square square in grid)
            {
                if(!square.tb.Text.Equals(""))
                {
                    square.set = true;
                    square.final = int.Parse(square.tb.Text);
                    square.possibilities.Clear();
                    square.TrimPossibilities(grid);
                }
               
            }
            recheck:
            updated = false;
            foreach (Section s in Sections)
            {
                updated |= s.ScanForPairs(updated);
                updated |= s.ScanForOnlySection(Sections, updated);
                updated |= s.CheckForHiddenPairs(updated);
                updated |= s.ScanForOnlyPlace(updated);
                updated |= s.CheckForPointingPairs(Sections, updated);
            }


            //fill squares and trim the squares they effect
            foreach (Square square in grid)
            {
                if (square.possibilities.Count ==0 && !square.set)
                {
                   // MessageBox.Show("A square with no possible solutions has been detected.");
                    square.tb.Background = Brushes.Red;
                }
                if (square.possibilities.Count == 1 && !square.set)
                {
                    square.set = true;
                    square.final = square.possibilities[0];
                    square.tb.Text = square.possibilities[0].ToString();
                    square.TrimPossibilities(grid);
                    Debug.WriteLine("Square Solved");
                }

            }


            iterations++;
            if(updated && iterations<maxIterations )
            {
                goto recheck;
            }
            else
            {

                foreach (Square s in grid)
                {
                    if (!s.set)
                    {
                        s.tb.FontSize = 8;
                        string temp = "";

                        
                        foreach (int i in s.possibilities)
                        {
                            temp += i.ToString() + ",";
                        }
                        s.tb.Text = temp;
                    }
                }

                MessageBox.Show("unable to continue");
            }            

        }

        //button to clear possibilitites
        private void ClearPossibiliites_Click(object sender, RoutedEventArgs e)
        {
            foreach (Square square in grid)
            {
                if(!square.set)
                {
                    square.tb.Text = "";
                    square.tb.FontSize= 32;
                }
            }

        }

        //debug function to show which sections a text box is in
        private void DisplaySections_Click(object sender, RoutedEventArgs e)
        {
            foreach(Square square in grid)
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

      
    }

 
}
