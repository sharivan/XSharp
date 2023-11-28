using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Matrix = XSharp.Math.Geometry.Matrix;
using Matrix3x2 = XSharp.Math.Geometry.Matrix3x2;
using Matrix5x4 = XSharp.Math.Geometry.Matrix5x4;
using DX9Matrix = SharpDX.Matrix;
using DX9Matrix3x2 = SharpDX.Matrix3x2;
using DX9Matrix5x4 = SharpDX.Matrix5x4;

namespace XSharp.Interop;

public static class TransformExtensions
{
    public static DX9Matrix ToDX9Matrix(this Matrix matrix)
    {
        return new DX9Matrix(
            matrix.M11, matrix.M12, matrix.M13, matrix.M14,
            matrix.M21, matrix.M22, matrix.M23, matrix.M24,
            matrix.M31, matrix.M32, matrix.M33, matrix.M34, 
            matrix.M41, matrix.M42, matrix.M43, matrix.M44
            );
    }

    public static Matrix ToMatrix(this DX9Matrix matrix)
    {
        return new Matrix(
            matrix.M11, matrix.M12, matrix.M13, matrix.M14,
            matrix.M21, matrix.M22, matrix.M23, matrix.M24,
            matrix.M31, matrix.M32, matrix.M33, matrix.M34,
            matrix.M41, matrix.M42, matrix.M43, matrix.M44
            );
    }
}