using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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