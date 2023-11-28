using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NAudio.Wave;

namespace XSharp.Engine.Sound;

public class NAudioWaveStreamFactory : WaveStreamFactory
{
    public override IWaveStream FromStream(Stream stream)
    {
        string tempFileName = null;
        try
        {
            tempFileName = Path.GetTempFileName();
            using (var fileStream = new FileStream(tempFileName, FileMode.Create, FileAccess.Write))
            {
                stream.CopyTo(fileStream);
            }

            return FromFile(tempFileName);
        }
        finally
        {
            if (tempFileName != null)
                File.Delete(tempFileName);
        }
    }

    public override IWaveStream FromFile(string path)
    {
        return new NAudioWaveStream(new MediaFoundationReader(path));
    }

    public override long TimeToBytePosition(IWaveStream stream, double time)
    {
        var impl = (NAudioWaveStream) stream;
        var position = (long) (time * impl.WaveFormat.AverageBytesPerSecond);
        position -= position % impl.WaveFormat.BlockAlign;
        return position;
    }
}