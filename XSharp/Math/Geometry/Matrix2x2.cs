using System.Runtime.InteropServices;

namespace XSharp.Math.Geometry;

/// <summary>
/// Uma matriz quadrada de ordem 2
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public readonly struct Matrix2x2
{
    /// <summary>
    /// Matriz nula
    /// </summary>
    public static readonly Matrix2x2 NULL_MATRIX = new(0, 0, 0, 0);
    /// <summary>
    /// Matriz identidade
    /// </summary>
    public static readonly Matrix2x2 IDENTITY = new(1, 0, 0, 1);

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4 * 4)]
    private readonly int[] elements;

    /// <summary>
    /// Cria uma matriz a partir de um array de valores numéricos
    /// </summary>
    /// <param name="values"></param>
    public Matrix2x2(params FixedSingle[] values)
    {
        elements = new int[4];
        elements[0] = values[0].RawValue;
        elements[1] = values[1].RawValue;
        elements[2] = values[2].RawValue;
        elements[3] = values[3].RawValue;
    }

    /// <summary>
    /// Cria uma matriz a partir de dois vetores
    /// </summary>
    /// <param name="v1"></param>
    /// <param name="v2"></param>
    public Matrix2x2(Vector v1, Vector v2)
    {
        elements = new int[4];
        elements[0] = v1.X.RawValue;
        elements[1] = v1.Y.RawValue;
        elements[2] = v2.X.RawValue;
        elements[3] = v2.Y.RawValue;
    }

    public FixedSingle Element00 => FixedSingle.FromRawValue(elements[0]);

    public FixedSingle Element01 => FixedSingle.FromRawValue(elements[1]);

    public FixedSingle Element10 => FixedSingle.FromRawValue(elements[2]);

    public FixedSingle Element11 => FixedSingle.FromRawValue(elements[3]);

    public FixedSingle GetElement(int i, int j)
    {
        return FixedSingle.FromRawValue(elements[2 * i + j]);
    }

    /// <summary>
    /// Calcula o determinante da uma matriz
    /// </summary>
    /// <returns>Determinante</returns>
    public FixedDouble Determinant()
    {
        return (FixedDouble) Element00 * (FixedDouble) Element11 - (FixedDouble) Element10 * (FixedDouble) Element01;
    }

    /// <summary>
    /// Transpõe a matriz
    /// </summary>
    /// <returns>Transposta da matriz</returns>
    public Matrix2x2 Transpose()
    {
        return new(Element00, Element10, Element01, Element11);
    }

    /// <summary>
    /// Inverte a matriz
    /// </summary>
    /// <returns>Inversa da matriz</returns>
    public Matrix2x2 Inverse()
    {
        FixedDouble determinant = Determinant();
        return new Matrix2x2(
                (FixedSingle) (Element01 / determinant),
                (FixedSingle) (-Element01 / determinant),
                (FixedSingle) (-Element10 / determinant),
                (FixedSingle) (Element00 / determinant)
            );
    }

    /// <summary>
    /// Soma entre duas matrizes
    /// </summary>
    /// <param name="m1">Primeira matriz</param>
    /// <param name="m2">Segunda matriz</param>
    /// <returns>Soma</returns>
    public static Matrix2x2 operator +(Matrix2x2 m1, Matrix2x2 m2)
    {
        return new(m1.Element00 + m2.Element00, m1.Element01 + m2.Element01, m1.Element10 + m2.Element10, m1.Element11 + m2.Element11);
    }

    /// <summary>
    /// Diferença/Subtração entre duas matrizes
    /// </summary>
    /// <param name="m1">Primeira matriz</param>
    /// <param name="m2">Segunda matriz</param>
    /// <returns>Diferença</returns>
    public static Matrix2x2 operator -(Matrix2x2 m1, Matrix2x2 m2)
    {
        return new(m1.Element00 - m2.Element00, m1.Element01 - m2.Element01, m1.Element10 - m2.Element10, m1.Element11 - m2.Element11);
    }

    /// <summary>
    /// Oposto aditivo de uma matriz
    /// </summary>
    /// <param name="m">Matriz</param>
    /// <returns>Oposto</returns>
    public static Matrix2x2 operator -(Matrix2x2 m)
    {
        return new(-m.Element00, -m.Element01, -m.Element10, -m.Element11);
    }

    /// <summary>
    /// Produto de uma matriz por um escalar
    /// </summary>
    /// <param name="factor">Escalar</param>
    /// <param name="m">Matriz</param>
    /// <returns>Produto</returns>
    public static Matrix2x2 operator *(FixedSingle factor, Matrix2x2 m)
    {
        return new(factor * m.Element00, factor * m.Element01, factor * m.Element10, factor * m.Element11);
    }

    /// <summary>
    /// Produto de uma matriz por um escalar
    /// </summary>
    /// <param name="m">Matriz</param>
    /// <param name="factor">Escalar</param>
    /// <returns>Produto</returns>
    public static Matrix2x2 operator *(Matrix2x2 m, FixedSingle factor)
    {
        return new(m.Element00 * factor, m.Element01 * factor, m.Element10 * factor, m.Element11 * factor);
    }

    /// <summary>
    /// Divisão de uma matriz por um escalar
    /// </summary>
    /// <param name="m">Matriz</param>
    /// <param name="divisor">Escalar</param>
    /// <returns>Divisão</returns>
    public static Matrix2x2 operator /(Matrix2x2 m, FixedSingle divisor)
    {
        return new(m.Element00 / divisor, m.Element01 / divisor, m.Element10 / divisor, m.Element11 / divisor);
    }

    /// <summary>
    /// Produto entre duas matrizes
    /// </summary>
    /// <param name="m1">Primeira matriz</param>
    /// <param name="m2">Segunda matriz</param>
    /// <returns>Produto matricial</returns>
    public static Matrix2x2 operator *(Matrix2x2 m1, Matrix2x2 m2)
    {
        return new(m1.Element00 * m2.Element00 + m1.Element01 * m2.Element10, m1.Element00 * m2.Element01 + m1.Element01 * m2.Element11,
                             m1.Element10 * m2.Element00 + m1.Element11 * m2.Element10, m1.Element10 * m2.Element01 + m1.Element11 * m2.Element11);
    }

    /// <summary>
    /// Calcula a matriz de rotação a partir de um angulo dado
    /// </summary>
    /// <param name="angle">Angulo em radianos</param>
    /// <returns>Matriz de rotação</returns>
    public static Matrix2x2 RotationMatrix(FixedSingle angle)
    {
        FixedSingle cos = System.Math.Cos(angle);
        FixedSingle sin = System.Math.Sin(angle);
        return new Matrix2x2(cos, -sin, sin, cos);
    }
}