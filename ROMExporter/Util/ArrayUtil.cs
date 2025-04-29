using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XSharp.Exporter.Util;

internal class ArrayUtil
{
    public static T[,] CreateFilledArray<T>(int width, int height, T value)
    {
        T[,] array = new T[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
                array[x, y] = value;
        }

        return array;
    }
}