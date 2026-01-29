using System;
using System.Runtime.InteropServices;

namespace XSharp.Math.Geometry;

/// <summary>
/// Structure using the same layout than <see cref="System.Drawing.SizeF"/>.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="Size2F"/> struct.
/// </remarks>
/// <param name="width">The x.</param>
/// <param name="height">The y.</param>
[StructLayout(LayoutKind.Sequential)]
public struct Size2F(float width, float height) : IEquatable<Size2F>
{
    /// <summary>
    /// A zero size with (width, height) = (0,0)
    /// </summary>
    public static readonly Size2F Zero = new(0, 0);

    /// <summary>
    /// A zero size with (width, height) = (0,0)
    /// </summary>
    public static readonly Size2F Empty = Zero;

    /// <summary>
    /// Width.
    /// </summary>
    public float Width = width;

    /// <summary>
    /// Height.
    /// </summary>
    public float Height = height;

    /// <summary>
    /// Determines whether the specified <see cref="System.Object"/> is equal to this instance.
    /// </summary>
    /// <param name="other">The <see cref="System.Object"/> to compare with this instance.</param>
    /// <returns>
    ///   <c>true</c> if the specified <see cref="System.Object"/> is equal to this instance; otherwise, <c>false</c>.
    /// </returns>
    public bool Equals(Size2F other)
    {
        return other.Width == Width && other.Height == Height;
    }

    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
        if (!(obj is Size2F))
            return false;

        return Equals((Size2F) obj);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        unchecked
        {
            return (Width.GetHashCode() * 397) ^ Height.GetHashCode();
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
    public static bool operator ==(Size2F left, Size2F right)
    {
        return left.Equals(right);
    }

    /// <summary>
    /// Implements the operator !=.
    /// </summary>
    /// <param name="left">The left.</param>
    /// <param name="right">The right.</param>
    /// <returns>
    /// The result of the operator.
    /// </returns>
    public static bool operator !=(Size2F left, Size2F right)
    {
        return !left.Equals(right);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return string.Format("({0},{1})", Width, Height);
    }
}