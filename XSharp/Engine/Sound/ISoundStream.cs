using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using XSharp.Serialization;

namespace XSharp.Engine.Sound;

public interface ISoundStream : IWaveStream, ISerializable
{
    public static WaveStreamFactory WaveStreamUtil => BaseEngine.Engine.WaveStreamUtil;

    public PrecachedSound Source
    {
        get;
        set;
    }

    public long StopPoint
    {
        get;
        set;
    }

    public long LoopPoint
    {
        get;
        set;
    }

    public bool Looping => LoopPoint >= 0;

    public bool Playing
    {
        get;
        set;
    }

    public void Reset();

    public void Play();

    public void Stop();

    public void UpdateSource(PrecachedSound source, long stopPoint, long loopPoint, bool ignoreUpdatesUntilPlayed = false);

    public void UpdateSource(PrecachedSound source, long loopPoint, bool ignoreUpdatesUntilPlayed = false)
    {
        UpdateSource(source, -1, loopPoint, ignoreUpdatesUntilPlayed);
    }

    public void UpdateSource(PrecachedSound source, bool ignoreUpdatesUntilPlayed = false)
    {
        UpdateSource(source, -1, -1, ignoreUpdatesUntilPlayed);
    }

    public void UpdateSource(PrecachedSound source, double stopTime, double loopTime, bool ignoreUpdatesUntilPlayed = false)
    {
        UpdateSource(source, stopTime >= 0 ? WaveStreamUtil.TimeToBytePosition(source.Stream, stopTime) : -1, loopTime >= 0 ? WaveStreamUtil.TimeToBytePosition(source.Stream, loopTime) : -1, ignoreUpdatesUntilPlayed);
    }

    public void UpdateSource(PrecachedSound source, double loopTime, bool ignoreUpdatesUntilPlayed = false)
    {
        UpdateSource(source, -1, loopTime, ignoreUpdatesUntilPlayed);
    }
}