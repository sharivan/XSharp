namespace XSharp.Engine.World;

public readonly struct Cell(int row, int col)
{
    public int Row
    {
        get;
    } = row;

    public int Col
    {
        get;
    } = col;

    public Cell((int, int) tuple) : this(tuple.Item1, tuple.Item2) { }

    public override int GetHashCode()
    {
        return 65536 * Row + Col;
    }

    public override bool Equals(object obj)
    {
        if (obj == null)
            return false;

        if (obj is not Cell)
            return false;

        var other = (Cell) obj;
        return other.Row == Row && other.Col == Col;
    }

    public override string ToString()
    {
        return Row + "," + Col;
    }

    public static implicit operator Cell((int, int) tuple)
    {
        return new(tuple.Item1, tuple.Item2);
    }

    public static implicit operator (int, int)(Cell cell)
    {
        return (cell.Row, cell.Col);
    }

    public void Deconstruct(out int row, out int col)
    {
        row = Row;
        col = Col;
    }

    public static bool operator ==(Cell left, Cell right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Cell left, Cell right)
    {
        return !(left == right);
    }
}