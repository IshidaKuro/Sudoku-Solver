using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;

namespace Sudoku_Solver
{
    class Square
    {
        public TextBox tb;
        public bool set;
        public List<int> possibilities = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
        public int row, column, section, final;
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
            sections = new int[] { section, x + 9, y + 18 };

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
}
