using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NAudio.Wave;

using XSharp.Serialization;

namespace XSharp.Engine.Sound;

public class NAudioSoundStream : WaveStream, ISoundStream
{
    private SoundStreamHelper helper;

    public PrecachedSound Source
    { 
        get => helper.Source; 
        set => helper.Source = value; 
    }

    /// <summary>
    /// Return source stream's wave format
    /// </summary>
    public override WaveFormat WaveFormat => ((NAudioWaveStream) helper.Source.Stream).WaveFormat;

    public override long Length => helper.Length;

    public override long Position
    { 
        get => helper.Position; 
        set => helper.Position = value; 
    }

    public long StopPoint
    { 
        get => helper.StopPoint; 
        set => helper.StopPoint = value; 
    }

    public long LoopPoint 
    { 
        get => helper.LoopPoint; 
        set => helper.LoopPoint = value; 
    }

    public bool Playing 
    { 
        get => helper.Playing; 
        set => helper.Playing = value; 
    }

    public NAudioSoundStream()
    {
        helper = new SoundStreamHelper();
    }

    public NAudioSoundStream(PrecachedSound source, long stopPoint, long loopPoint)
    {
        helper = new SoundStreamHelper(source, stopPoint, loopPoint);
    }

    public NAudioSoundStream(PrecachedSound source, double stopTime, double loopTime)
    {
        helper = new SoundStreamHelper(source, stopTime, loopTime);
    }

    public NAudioSoundStream(PrecachedSound source, long loopPoint)
    {
        helper = new SoundStreamHelper(source, loopPoint);
    }

    public NAudioSoundStream(PrecachedSound source)
    {
        helper = new SoundStreamHelper(source);
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        return helper.Read(buffer, offset, count);
    }

    public void Reset()
    {
        helper.Reset();
    }

    public void Play()
    {
        helper.Play();
    }

    public void Stop()
    {
        helper.Stop();
    }

    public void UpdateSource(PrecachedSound source, long stopPoint, long loopPoint, bool ignoreUpdatesUntilPlayed = false)
    {
        helper.UpdateSource(source, stopPoint, loopPoint, ignoreUpdatesUntilPlayed);
    }

    public void UpdateSource(PrecachedSound source, long loopPoint, bool ignoreUpdatesUntilPlayed = false)
    {
        helper.UpdateSource(source, loopPoint, ignoreUpdatesUntilPlayed);
    }

    public void UpdateSource(PrecachedSound source, bool ignoreUpdatesUntilPlayed = false)
    {
        helper.UpdateSource(source, ignoreUpdatesUntilPlayed);
    }

    public void UpdateSource(PrecachedSound source, double stopTime, double loopTime, bool ignoreUpdatesUntilPlayed = false)
    {
        helper.UpdateSource(source, stopTime, loopTime, ignoreUpdatesUntilPlayed);
    }

    public void UpdateSource(PrecachedSound source, double loopTime, bool ignoreUpdatesUntilPlayed = false)
    {
        helper.UpdateSource(source, loopTime, ignoreUpdatesUntilPlayed);
    }

    public void Deserialize(ISerializer serializer)
    {
        helper.Deserialize(serializer);
    }

    public void Serialize(ISerializer serializer)
    {
        helper.Serialize(serializer);
    }

    public new void Dispose()
    {
        helper?.Dispose();
    }
}