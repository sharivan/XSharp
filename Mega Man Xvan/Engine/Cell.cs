using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMX.Engine
{
    public struct Cell
    {
        private int row;
        private int col;

        public int Row
        {
            get
            {
                return row;
            }
        }

        public int Col
        {
            get
            {
                return col;
            }
        }

        public Cell(int row, int col)
        {
            this.row = row;
            this.col = col;
        }

        public override int GetHashCode()
        {
            return 65536 * row + col;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (!(obj is Cell))
                return false;

            Cell other = (Cell) obj;
            return other.row == row && other.col == col;
        }

        public override string ToString()
        {
            return row + "," + col;
        }
    }
}
