using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace XSharp.Math.Geometry;

/// <summary>
/// Structure using the same layout than <see cref="System.Drawing.SizeF"/>.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct Size2F : IEquatable<Size2F>
{
    /// <summary>
    /// A zero size with (width, height) = (0,0)
    /// </summary>
    public static readonly Size2F Zero = new Size2F(0, 0);

    /// <summary>
    /// A zero size with (width, height) = (0,0)
    /// </summary>
    public static readonly Size2F Empty = Zero;

    /// <summary>
    /// Initializes a new instance of the <see cref="Size2F"/> struct.
    /// </summary>
    /// <param name="width">The x.</param>
    /// <param name="height">The y.</param>
    public Size2F(float width, float height)
    {
        Width = width;
        Height = height;
    }

    /// <summary>
    /// Width.
    /// </summary>
    public float Width;

    /// <summary>
    /// Height.
    /// </summary>
    public float Height;

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