using System;
using System.Collections.Generic;

using XSharp.Engine.Collision;
using XSharp.Engine.Entities;
using XSharp.Math;
using XSharp.Math.Geometry;
using XSharp.Serialization;

namespace XSharp.Engine;

internal class Partition<T>(Box box, int rows, int cols) : ISerializable where T : Entity
{
    private class PartitionCell<U> : ISerializable where U : Entity
    {
        Box box;
        EntitySet<U> values;

        public int Count => values.Count;

        public PartitionCell()
        {
            values = [];
        }

        public PartitionCell(Box box)
        {
            this.box = box;

            values = [];
        }

        public void Insert(U value)
        {
            values.Add(value);
        }

        public void Query(Vector v, EntitySet<U> result, U exclude, ICollection<U> addictionalExclusionList, bool aliveOnly = true)
        {
            if (!box.Contains(v))
                return;

            foreach (U value in values)
            {
                if (value == null || exclude != null && exclude.Equals(value))
                    continue;

                if (addictionalExclusionList != null && addictionalExclusionList.Contains(value))
                    continue;

                if (value.Hitbox.RoundOriginToFloor().Contains(v) && (!aliveOnly || value.Alive && !value.MarkedToRemove))
                    result.Add(value);
            }
        }

        public void Query(LineSegment line, EntitySet<U> result, U exclude, ICollection<U> addictionalExclusionList, bool aliveOnly = true)
        {
            if (!box.HasIntersectionWith(line))
                return;

            foreach (U value in values)
            {
                if (value == null || exclude != null && exclude.Equals(value))
                    continue;

                if (addictionalExclusionList != null && addictionalExclusionList.Contains(value))
                    continue;

                if (value.Hitbox.RoundOriginToFloor().HasIntersectionWith(line) && (!aliveOnly || value.Alive && !value.MarkedToRemove)) // Se a intersecção for não vazia e se a entidade ainda não estiver na lista de resultados
                    result.Add(value);
            }
        }

        public void Query(IGeometry geometry, EntitySet<U> result, U exclude, ICollection<U> addictionalExclusionList, bool aliveOnly = true)
        {
            if (!geometry.HasIntersectionWith(box))
                return;

            foreach (U value in values)
            {
                if (value == null || exclude != null && exclude.Equals(value))
                    continue;

                if (addictionalExclusionList != null && addictionalExclusionList.Contains(value))
                    continue;

                if (geometry.HasIntersectionWith(value.Hitbox.RoundOriginToFloor()) && (!aliveOnly || value.Alive && !value.MarkedToRemove)) // Se a intersecção for não vazia e se a entidade ainda não estiver na lista de resultados
                    result.Add(value);
            }
        }

        public void Query(Box box, EntitySet<U> result, U exclude, ICollection<U> addictionalExclusionList, bool aliveOnly = true)
        {
            if (!box.IsOverlaping(this.box))
                return;

            foreach (U value in values)
            {
                if (value == null || exclude != null && exclude.Equals(value))
                    continue;

                if (addictionalExclusionList != null && addictionalExclusionList.Contains(value))
                    continue;

                if (value.Hitbox.RoundOriginToFloor().IsOverlaping(box) && (!aliveOnly || value.Alive && !value.MarkedToRemove))
                    result.Add(value);
            }
        }

        public void Update(U value)
        {
            if (value.Hitbox.RoundOriginToFloor().IsOverlaping(box))
                values.Add(value);
            else
                values.Remove(value);
        }

        public void Remove(U value)
        {
            values.Remove(value);
        }

        public void Clear()
        {
            values.Clear();
        }

        public void Deserialize(ISerializer reader)
        {
            box = reader.ReadBox();

            values ??= [];
            values.Deserialize(reader);
        }

        public void Serialize(ISerializer writer)
        {
            writer.WriteBox(box);
            values.Serialize(writer);
        }
    }

    private Box box = box;
    private int rows = rows;
    private int cols = cols;

    private PartitionCell<T>[,] grid = new PartitionCell<T>[cols, rows];
    private FixedSingle cellWidth = box.Width / cols;
    private FixedSingle cellHeight = box.Height / rows;

    public Partition(FixedSingle left, FixedSingle top, FixedSingle width, FixedSingle height, int rows, int cols)
        : this(new Box(new Vector(left, top), Vector.NULL_VECTOR, new Vector(width, height)), rows, cols)
    {
    }

    public void Insert(T item)
    {
        Box box = item.Hitbox.RoundOriginToFloor();

        Vector lt = this.box.LeftTop;
        Vector rb = this.box.RightBottom;

        Vector queryLT = box.LeftTop;
        Vector queryRB = box.RightBottom;

        int startCol = ((queryLT.X - lt.X) / cellWidth).Floor();
        if (startCol < 0)
            startCol = 0;

        int startRow = ((queryLT.Y - lt.Y) / cellHeight).Floor();
        if (startRow < 0)
            startRow = 0;

        int endCol = ((queryRB.X - lt.X - 1) / cellWidth).Ceil();
        if (endCol >= cols)
            endCol = cols - 1;

        int endRow = ((queryRB.Y - lt.Y - 1) / cellHeight).Ceil();
        if (endRow >= rows)
            endRow = rows - 1;

        for (int col = startCol; col <= endCol; col++)
        {
            for (int row = startRow; row <= endRow; row++)
            {
                var cellBox = new Box((lt.X + cellWidth * col, lt.Y + cellHeight * row), Vector.NULL_VECTOR, (cellWidth, cellHeight));

                if (!cellBox.IsOverlaping(box))
                    continue;

                if (grid[col, row] == null)
                    grid[col, row] = new PartitionCell<T>(cellBox);

                grid[col, row].Insert(item);
            }
        }
    }

    public int Query(EntitySet<T> resultSet, Vector v, bool aliveOnly = true)
    {
        return Query(resultSet, v, null, null, aliveOnly);
    }

    public int Query(EntitySet<T> resultSet, Vector v, T exclude, bool aliveOnly = true)
    {
        return Query(resultSet, v, exclude, null, aliveOnly);
    }

    public int Query(EntitySet<T> resultSet, Vector v, ICollection<T> exclusionList, bool aliveOnly = true)
    {
        return Query(resultSet, v, null, exclusionList, aliveOnly);
    }

    public int Query(EntitySet<T> resultSet, Vector v, T exclude, ICollection<T> addictionalExclusionList, bool aliveOnly = true)
    {
        Vector lt = box.LeftTop;

        int col = ((v.X - lt.X) / cellWidth).Floor();

        if (col < 0)
            col = 0;

        int row = ((v.Y - lt.Y) / cellHeight).Floor();

        if (row < 0)
            row = 0;

        grid[col, row]?.Query(v, resultSet, exclude, addictionalExclusionList, aliveOnly);

        return resultSet.Count;
    }

    public int Query(EntitySet<T> resultSet, LineSegment line, bool aliveOnly = true)
    {
        return Query(resultSet, line, null, null, aliveOnly);
    }

    public int Query(EntitySet<T> resultSet, LineSegment line, T exclude, bool aliveOnly = true)
    {
        return Query(resultSet, line, exclude, null, aliveOnly);
    }

    public int Query(EntitySet<T> resultSet, LineSegment line, ICollection<T> exclusionList, bool aliveOnly = true)
    {
        return Query(resultSet, line, null, exclusionList, aliveOnly);
    }

    public int Query(EntitySet<T> resultSet, LineSegment line, T exclude, ICollection<T> addictionalExclusionList, bool aliveOnly = true)
    {
        var type = box.Intersection(line, out line);
        if (type == GeometryType.EMPTY)
            return 0;

        var delta = line.End - line.Start;
        var stepVector = CollisionChecker.GetStepVectorHorizontal(delta, cellWidth);
        var tracingDistance = FixedSingle.Max(delta.X.Abs, delta.Y.Abs);

        var testVector = line.Start;
        FixedSingle distance = 0;
        for (int j = 0; distance <= tracingDistance; distance += cellWidth, testVector = line.Start + stepVector * j)
        {
            int col = (int) (testVector.X / cellWidth);
            int row = (int) (testVector.Y / cellHeight);

            if (row < 0 || row >= rows || col < 0 || col >= cols)
                continue;

            grid[col, row]?.Query(box, resultSet, exclude, addictionalExclusionList, aliveOnly);
        }

        return resultSet.Count;
    }

    public int Query(EntitySet<T> resultSet, Parallelogram parallelogram, bool aliveOnly = true)
    {
        return Query(resultSet, parallelogram, null, null, aliveOnly);
    }

    public int Query(EntitySet<T> resultSet, Parallelogram parallelogram, T exclude, bool aliveOnly = true)
    {
        return Query(resultSet, parallelogram, exclude, null, aliveOnly);
    }

    public int Query(EntitySet<T> resultSet, Parallelogram parallelogram, ICollection<T> exclusionList, bool aliveOnly = true)
    {
        return Query(resultSet, parallelogram, null, exclusionList, aliveOnly);
    }

    public int Query(EntitySet<T> resultSet, Parallelogram parallelogram, T exclude, ICollection<T> addictionalExclusionList, bool aliveOnly = true)
    {
        Vector stepVector = CollisionChecker.GetStepVectorHorizontal(parallelogram.Direction, cellWidth);
        FixedSingle stepDistance = stepVector.Length;
        if (stepDistance == 0)
            stepDistance = cellWidth;

        FixedSingle tracingDistance = parallelogram.Direction.Length;
        var tracingBox = new Box(parallelogram.Origin, cellWidth, parallelogram.SmallerHeight);

        for (FixedSingle distance = 0; distance <= tracingDistance; distance += stepDistance, tracingBox += stepVector)
        {
            int startCol = (tracingBox.Left / cellWidth).Floor();
            int startRow = (tracingBox.Top / cellHeight).Floor();

            int endCol = (tracingBox.Right / cellWidth).Ceil();
            int endRow = (tracingBox.Bottom / cellHeight).Ceil();

            if (startCol < 0)
                startCol = 0;

            if (startRow < 0)
                startRow = 0;

            if (endCol >= cols)
                endCol = cols - 1;

            if (endRow >= rows)
                endRow = rows - 1;

            for (int col = startCol; col <= endCol; col++)
            {
                for (int row = startRow; row <= endRow; row++)
                    grid[col, row]?.Query(parallelogram, resultSet, exclude, addictionalExclusionList, aliveOnly);
            }
        }

        return resultSet.Count;
    }

    public int Query(EntitySet<T> resultSet, Box box, bool aliveOnly = true)
    {
        return Query(resultSet, box, null, null, aliveOnly);
    }

    public int Query(EntitySet<T> resultSet, Box box, T exclude, bool aliveOnly = true)
    {
        return Query(resultSet, box, exclude, null, aliveOnly);
    }

    public int Query(EntitySet<T> resultSet, Box box, ICollection<T> exclusionList, bool aliveOnly = true)
    {
        return Query(resultSet, box, null, exclusionList, aliveOnly);
    }

    public int Query(EntitySet<T> resultSet, Box box, T exclude, ICollection<T> addictionalExclusionList, bool aliveOnly = true)
    {
        Vector lt = this.box.LeftTop;

        Vector queryLT = box.LeftTop;
        Vector queryRB = box.RightBottom;

        int startCol = ((queryLT.X - lt.X) / cellWidth).Floor();

        if (startCol < 0)
            startCol = 0;

        int startRow = ((queryLT.Y - lt.Y) / cellHeight).Floor();

        if (startRow < 0)
            startRow = 0;

        int endCol = ((queryRB.X - lt.X - 1) / cellWidth).Ceil();

        if (endCol >= cols)
            endCol = cols - 1;

        int endRow = ((queryRB.Y - lt.Y - 1) / cellHeight).Ceil();

        if (endRow >= rows)
            endRow = rows - 1;

        for (int col = startCol; col <= endCol; col++)
        {
            for (int row = startRow; row <= endRow; row++)
                grid[col, row]?.Query(box, resultSet, exclude, addictionalExclusionList, aliveOnly);
        }

        return resultSet.Count;
    }

    public void Update(T item, bool force = false)
    {
        Box lastBox = item.GetLastBox(BoxKind.HITBOX);
        Box box = item.Hitbox.RoundOriginToFloor();

        if (!force && lastBox == box)
            return;

        Vector lt = this.box.LeftTop;
        Vector rb = this.box.RightBottom;

        Vector queryLastLT = lastBox.LeftTop;
        Vector queryLastRB = lastBox.RightBottom;

        Vector queryLT = box.LeftTop;
        Vector queryRB = box.RightBottom;

        int startCol = ((FixedSingle.Min(queryLastLT.X, queryLT.X) - lt.X) / cellWidth).Floor();

        if (startCol < 0)
            startCol = 0;

        if (startCol >= cols)
            startCol = cols - 1;

        int startRow = ((FixedSingle.Min(queryLastLT.Y, queryLT.Y) - lt.Y) / cellHeight).Floor();

        if (startRow < 0)
            startRow = 0;

        if (startRow >= rows)
            startRow = rows - 1;

        int endCol = ((FixedSingle.Max(queryLastRB.X, queryRB.X) - lt.X - 1) / cellWidth).Ceil();

        if (endCol < 0)
            endCol = 0;

        if (endCol >= cols)
            endCol = cols - 1;

        int endRow = ((FixedSingle.Max(queryLastRB.Y, queryRB.Y) - lt.Y - 1) / cellHeight).Ceil();

        if (endRow < 0)
            endRow = 0;

        if (endRow >= rows)
            endRow = rows - 1;

        for (int col = startCol; col <= endCol; col++)
        {
            for (int row = startRow; row <= endRow; row++)
            {
                if (grid[col, row] != null)
                {
                    grid[col, row].Update(item);

                    if (grid[col, row].Count == 0)
                        grid[col, row] = null;
                }
                else
                {
                    var cellBox = new Box(new Vector(lt.X + cellWidth * col, lt.Y + cellHeight * row), Vector.NULL_VECTOR, new Vector(cellWidth, cellHeight));

                    if (!cellBox.IsOverlaping(box))
                        continue;

                    if (grid[col, row] == null)
                        grid[col, row] = new PartitionCell<T>(cellBox);

                    grid[col, row].Insert(item);
                }
            }
        }
    }

    public void Remove(T item)
    {
        Box box = item.Hitbox.RoundOriginToFloor();

        Vector lt = this.box.LeftTop;
        Vector rb = this.box.RightBottom;

        Vector queryLT = box.LeftTop;
        Vector queryRB = box.RightBottom;

        int startCol = ((queryLT.X - lt.X) / cellWidth).Floor();

        if (startCol < 0)
            startCol = 0;

        if (startCol >= cols)
            startCol = cols - 1;

        int startRow = ((queryLT.Y - lt.Y) / cellHeight).Floor();

        if (startRow < 0)
            startRow = 0;

        if (startRow >= rows)
            startRow = rows - 1;

        int endCol = ((queryRB.X - lt.X - 1) / cellWidth).Ceil();

        if (endCol < 0)
            endCol = 0;

        if (endCol >= cols)
            endCol = cols - 1;

        int endRow = ((queryRB.Y - lt.Y - 1) / cellHeight).Ceil();

        if (endRow < 0)
            endRow = 0;

        if (endRow >= rows)
            endRow = rows - 1;

        for (int col = startCol; col <= endCol; col++)
        {
            for (int row = startRow; row <= endRow; row++)
            {
                if (grid[col, row] != null)
                {
                    grid[col, row].Remove(item);

                    if (grid[col, row].Count == 0)
                        grid[col, row] = null;
                }
            }
        }
    }

    public void Clear()
    {
        for (int col = 0; col < cols; col++)
        {
            for (int row = 0; row < rows; row++)
            {
                if (grid[col, row] != null)
                {
                    grid[col, row].Clear();
                    grid[col, row] = null;
                }
            }
        }
    }

    private void ResizeQuads(int rows, int cols)
    {
        var newArray = new PartitionCell<T>[rows, cols];
        int minRows = System.Math.Min(grid.GetLength(0), newArray.GetLength(0));
        int minCols = System.Math.Min(grid.GetLength(1), newArray.GetLength(1));

        for (int i = 0; i < minCols; i++)
            Array.Copy(grid, i * grid.GetLength(0), newArray, i * newArray.GetLength(0), minRows);

        grid = newArray;
    }

    public void Deserialize(ISerializer serializer)
    {
        box = serializer.ReadBox();
        rows = serializer.ReadInt();
        cols = serializer.ReadInt();

        if (grid == null)
            grid = new PartitionCell<T>[rows, cols];
        else
            ResizeQuads(rows, cols);

        cellWidth = serializer.ReadFixedSingle();
        cellHeight = serializer.ReadFixedSingle();

        for (int col = 0; col < cols; col++)
        {
            for (int row = 0; row < rows; row++)
            {
                bool isSet = serializer.ReadBool();
                if (isSet)
                {
                    var cell = new PartitionCell<T>();
                    cell.Deserialize(serializer);
                    grid[row, col] = cell;
                }
                else
                    grid[row, col] = null;
            }
        }
    }

    public void Serialize(ISerializer serializer)
    {
        serializer.WriteBox(box);
        serializer.WriteInt(rows);
        serializer.WriteInt(cols);

        serializer.WriteFixedSingle(cellWidth);
        serializer.WriteFixedSingle(cellHeight);

        for (int col = 0; col < cols; col++)
        {
            for (int row = 0; row < rows; row++)
            {
                var cell = grid[row, col];
                if (cell == null)
                {
                    serializer.WriteBool(false);
                }
                else
                {
                    serializer.WriteBool(true);
                    cell.Serialize(serializer);
                }
            }
        }
    }
}