using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;



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
                //eliminate possible conflicts
                foreach (Square s in grid)
                {
                    if (this.section == s.section || this.row == s.row || this.column == s.column)
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
    public void ScanForPairs()
    {
        foreach(Square s in this.squares) //for each square in this section
        {
            if (!s.set && s.possibilities.Count ==2) //if only 2 things can go here
            {
                foreach(Square q in this.squares)
                {
                    if(!q.set && s!=q && s.possibilities.Equals(q.possibilities))
                    {
                        foreach(Square u in this.squares)
                        {
                            if(u!=s && u!=q) 
                            {
                                foreach (int i in s.possibilities)
                                {
                                    u.possibilities.Remove(i);
                                }
                            }
                        }
                    }
                }
            }
        }
    }


    //scan each section to see if there is only 1 place a number can go
    public void ScanForOnlyPlace()
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
                    }

                }
            }
        
            


        }
    }

    public void ScanForOnlySection(Section[] sections)
    {
        //if a number has to go on one line within a square, trim possibilities from the square excluding the line the number has to go in
        //or if a number has to go within a square on a line, trim possibilities from the line   ''      ''      ''      ''      ''      ''

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
                    }
                }
               
            }

        }
    }


}


namespace Sudoku_Solver
{
    
    public partial class MainWindow : Window
    {


        Square[,] grid = new Square[9, 9];

        Section[] Sections = new Section[27];
       
        public MainWindow()
        {
            InitializeComponent();

            for (int i = 0; i < Sections.Count(); i++)
            {
                Sections[i] = new Section(new List<Square>());
            }
            //initialize XAML components
           
            
           //for each space on the board
            for (int x = 0; x<9; x++)
            {
                for (int y = 0; y<9; y++)
                {
                    //create a square and create a text box to represent it
                    Square s = new Square(x, y);
                    TextBox t = s.tb;
                    t.FontSize = 32;
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

       
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            

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
            bool updated = false;
            foreach (Section s in Sections)
            {
                s.ScanForPairs();
                s.ScanForOnlyPlace();
                s.ScanForOnlySection(Sections);
            }

            //fill squares and trim the squares they effect
            foreach (Square square in grid)
            {
                if (square.possibilities.Count ==0 && !square.set)
                {
                    MessageBox.Show("A square with no possible solutions has been detected.");
                    break;
                }
                if (square.possibilities.Count == 1 && !square.set)
                {
                    updated = true;
                    square.set = true;
                    square.final = square.possibilities[0];
                    square.tb.Text = square.possibilities[0].ToString();
                    square.TrimPossibilities(grid);
                }

            }



            if(updated)
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
        protected void textBoxChanged(object sender, TextChangedEventArgs args)
        {
            //clear the text box if the character is not an integer
            
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
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

        private void Button_Click_2(object sender, RoutedEventArgs e)
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
