using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sudoku_Solver
{
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
        public bool ScanForPairs()
        {
            bool updated = false;
            foreach (Square s in this.squares) //for each square in this section
            {
                if (!s.set && s.possibilities.Count == 2) //if only 2 things can go here
                {
                    foreach (Square q in this.squares) //look at the other squares
                    {
                        if (!q.set && s != q && s.possibilities.SequenceEqual(q.possibilities)) //if they both have the same possible solutions
                        {
                            foreach (Square u in this.squares)
                            {
                                if (u != s && u != q)
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
        public bool ScanForOnlyPlace()
        {
            bool updated = false;
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
                if (count == 1) // if this is the only place we can put that number in the section
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


        public bool ScanForOnlySection(Section[] sections)
        {

            bool updated = false;

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
                        else if (commonSection == -1)
                        {
                            if (!s.sections.Contains<int>(otherSection1) || !s.sections.Contains<int>(otherSection2))
                            {
                                //if a square is found that has neither of these as a section, break the loop as this number can go elsewhere
                                broken = true;
                                break;
                            }
                            else
                            {
                                if (s.sections.Contains<int>(otherSection1))
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
                            if (!s.sections.Contains<int>(commonSection))
                            {
                                broken = true;
                                break;
                            }
                        }
                    }

                }
                //if we iterare through all the squares and don't break the loop
                if (!broken && commonSection != -1)
                {
                    //we must have found the case we are looking for
                    foreach (Square s in this.squares)
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

        public bool CheckForHiddenPairs()
        {
            bool updated = false;
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

        public bool CheckForPointingPairs(Section[] sections)
        {
            bool updated = false;
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

        public bool ScanForObviousTriples()
        {
            bool updated = false;
            var candidates = squares.Where(s => !s.set && s.possibilities.Count <= 3).ToList();

            for (int i = 0; i < candidates.Count; i++)
            {
                for (int j = i + 1; j < candidates.Count; j++)
                {
                    for (int k = j + 1; k < candidates.Count; k++)
                    {
                        var combined = candidates[i].possibilities
                            .Union(candidates[j].possibilities)
                            .Union(candidates[k].possibilities)
                            .ToList();

                        if (combined.Count == 3)
                        {
                            var triple = combined;

                            foreach (Square s in squares)
                            {
                                if (!new[] { candidates[i], candidates[j], candidates[k] }.Contains(s))
                                {
                                    foreach (int val in triple)
                                    {
                                        if (s.possibilities.Remove(val))
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


    }
}
