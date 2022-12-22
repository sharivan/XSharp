using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMX.Engine.World
{
    public readonly struct Cell
    {
        public int Row { get; }

        public int Col { get; }

        public Cell(int row, int col)
        {
            this.Row = row;
            this.Col = col;
        }

        public override int GetHashCode() => 65536 * Row + Col;

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (obj is not Cell)
                return false;

            var other = (Cell) obj;
            return other.Row == Row && other.Col == Col;
        }

        public override string ToString() => Row + "," + Col;
    }
}
