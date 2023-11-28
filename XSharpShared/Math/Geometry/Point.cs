﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace XSharp.Math.Geometry;

/// <summary>
/// Structure using the same layout than <see cref="System.Drawing.Point"/>.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct Point : IEquatable<Point>
{
    /// <summary>
    /// A point with (0,0) coordinates.
    /// </summary>
    public static readonly Point Zero = new Point(0, 0);

    /// <summary>
    /// Initializes a new instance of the <see cref="Point"/> struct.
    /// </summary>
    /// <param name="x">The x.</param>
    /// <param name="y">The y.</param>
    public Point(int x, int y)
    {
        X = x;
        Y = y;
    }

    /// <summary>
    /// Left coordinate.
    /// </summary>
    public int X;

    /// <summary>
    /// Top coordinate.
    /// </summary>
    public int Y;

    /// <summary>
    /// Determines whether the specified <see cref="System.Object"/> is equal to this instance.
    /// </summary>
    /// <param name="other">The <see cref="System.Object"/> to compare with this instance.</param>
    /// <returns>
    ///   <c>true</c> if the specified <see cref="System.Object"/> is equal to this instance; otherwise, <c>false</c>.
    /// </returns>
    [MethodImpl((MethodImplOptions) 0x100)] // MethodImplOptions.AggressiveInlining
    public bool Equals(ref Point other)
    {
        return other.X == X && other.Y == Y;
    }

    /// <summary>
    /// Determines whether the specified <see cref="System.Object"/> is equal to this instance.
    /// </summary>
    /// <param name="other">The <see cref="System.Object"/> to compare with this instance.</param>
    /// <returns>
    ///   <c>true</c> if the specified <see cref="System.Object"/> is equal to this instance; otherwise, <c>false</c>.
    /// </returns>
    [MethodImpl((MethodImplOptions) 0x100)] // MethodImplOptions.AggressiveInlining
    public bool Equals(Point other)
    {
        return Equals(ref other);
    }

    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
        if (!(obj is Point))
            return false;

        var strongValue = (Point) obj;
        return Equals(ref strongValue);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        unchecked
        {
            return (X * 397) ^ Y;
        }
    }

    /// <summary>
    /// Implements the operator ==.
    /// </summary>
    /// <param name="left">The left.</param>
    /// <param name="right">The right.</param>
    /// <returns>
    /// The result of the operator.
    /// </returns>
    [MethodImpl((MethodImplOptions) 0x100)] // MethodImplOptions.AggressiveInlining
    public static bool operator ==(Point left, Point right)
    {
        return left.Equals(ref right);
    }

    /// <summary>
    /// Implements the operator !=.
    /// </summary>
    /// <param name="left">The left.</param>
    /// <param name="right">The right.</param>
    /// <returns>
    /// The result of the operator.
    /// </returns>
    [MethodImpl((MethodImplOptions) 0x100)] // MethodImplOptions.AggressiveInlining
    public static bool operator !=(Point left, Point right)
    {
        return !left.Equals(ref right);
    }

    public override string ToString()
    {
        return string.Format("({0},{1})", X, Y);
    }

    /// <summary>
    /// Performs an explicit conversion from <see cref="Vector2"/> to <see cref="Point"/>.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>The result of the conversion.</returns>
    public static explicit operator Point(Vector2 value)
    {
        return new Point((int) value.X, (int) value.Y);
    }

    /// <summary>
    /// Performs an implicit conversion from <see cref="Point"/> to <see cref="Vector2"/>.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>The result of the conversion.</returns>
    public static implicit operator Vector2(Point value)
    {
        return new Vector2(value.X, value.Y);
    }
}