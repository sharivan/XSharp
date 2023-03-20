using System.IO;

using NAudio.Wave;

namespace XSharp.Engine.Sound;

public static class WaveStreamUtil
{
    public static WaveStream FromStream(Stream stream)
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

    public static WaveStream FromFile(string path)
    {
        return new MediaFoundationReader(path);
    }

    public static long TimeToBytePosition(WaveStream stream, double time)
    {
        return (long) (time * stream.WaveFormat.AverageBytesPerSecond);
    }
}