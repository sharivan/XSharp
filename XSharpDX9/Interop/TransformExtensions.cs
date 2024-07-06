using DX9Matrix = SharpDX.Matrix;
using Matrix = XSharp.Math.Geometry.Matrix;

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