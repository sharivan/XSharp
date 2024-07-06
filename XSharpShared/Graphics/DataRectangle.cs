using System;

namespace XSharp.Graphics;

public struct DataRectangle
{
    public IntPtr DataPointer;

    public int Pitch;

    public DataRectangle(IntPtr dataPointer, int pitch)
    {
        DataPointer = dataPointer;
        Pitch = pitch;
    }
}