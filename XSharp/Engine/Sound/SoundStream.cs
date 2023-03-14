using System;
using System.IO;

using NAudio.Wave;

using XSharp.Serialization;

namespace XSharp.Engine.Sound;

public enum SoundFormat
{
    WAVE,
    MP3
}

public static class WaveStreamUtil
{
    public static WaveStream FromStream(Stream stream, SoundFormat format)
    {
        return format switch
        {
            SoundFormat.WAVE => new WaveFileReader(stream),
            SoundFormat.MP3 => new Mp3FileReader(stream),
            _ => throw new ArgumentException($"Sound format is invalid: {format}"),
        };
    }

    public static WaveStream FromFile(string waveFile, SoundFormat format)
    {
        return format switch
        {
            SoundFormat.WAVE => new WaveFileReader(waveFile),
            SoundFormat.MP3 => new Mp3FileReader(waveFile),
            _ => throw new ArgumentException($"Sound format is invalid: {format}"),
        };
    }

    public static long TimeToBytePosition(WaveStream stream, double time)
    {
        return (long) (time * stream.WaveFormat.AverageBytesPerSecond);
    }
}

/// <summary>
/// Stream for playback
/// </summary>
public class SoundStream : WaveStream, ISerializable
{
    private WaveEntry source;
    private bool ignoreUpdatesUntilPlayed;

    /// <summary>
    /// Return source stream's wave format
    /// </summary>
    public override WaveFormat WaveFormat => source.Stream.WaveFormat;

    public WaveEntry Source
    {
        get => source;
        set => UpdateSource(value);
    }

    /// <summary>
    /// LoopStream simply returns
    /// </summary>
    public override long Length => source.Stream.Length;

    /// <summary>
    /// LoopStream simply passes on positioning to source stream
    /// </summary>
    public override long Position
    {
        get => source.Stream.Position;
        set => source.Stream.Position = value;
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

    public SoundStream()
    {
        Playing = false;
    }

    public SoundStream(WaveEntry source, long stopPoint, long loopPoint)
    {
        UpdateSource(source, stopPoint, loopPoint);
        Playing = true;
    }

    public SoundStream(WaveEntry source, double stopTime, double loopTime) : this(source, stopTime >= 0 ? WaveStreamUtil.TimeToBytePosition(source.Stream, stopTime) : -1, loopTime >= 0 ? WaveStreamUtil.TimeToBytePosition(source.Stream, loopTime) : -1) { }

    public SoundStream(WaveEntry source, long loopPoint) : this(source, -1, loopPoint) { }

    public SoundStream(WaveEntry source) : this(source, -1, -1) { }

    public override int Read(byte[] buffer, int offset, int count)
    {
        if (!Playing)
            return 0;

        int totalBytesRead = 0;
        while (totalBytesRead < count)
        {
            int bytesToRead = count - totalBytesRead;
            if (source.Stream.Position + bytesToRead > StopPoint)
                bytesToRead = (int) (StopPoint - source.Stream.Position);

            if (bytesToRead < 0)
                bytesToRead = 0;

            int bytesRead = source.Stream.Read(buffer, offset + totalBytesRead, bytesToRead);
            if (bytesRead == 0)
            {
                if (source.Stream.Position == 0 || !Looping)
                {
                    // something wrong with the source stream
                    ignoreUpdatesUntilPlayed = false;
                    break;
                }

                // loop
                source.Stream.Position = LoopPoint;
            }

            totalBytesRead += bytesRead;
        }

        return totalBytesRead;
    }

    public void Reset()
    {
        source.Stream.Position = 0;
    }

    public void Play()
    {
        Playing = true;
    }

    public void Stop()
    {
        Playing = false;
        ignoreUpdatesUntilPlayed = false;
    }

    public void UpdateSource(WaveEntry source, long stopPoint, long loopPoint, bool ignoreUpdatesUntilPlayed = false)
    {
        if (this.ignoreUpdatesUntilPlayed)
            return;

        this.source = source;
        StopPoint = stopPoint >= 0 ? stopPoint : source.Stream.Length;
        LoopPoint = loopPoint;
        this.ignoreUpdatesUntilPlayed = ignoreUpdatesUntilPlayed;
    }

    public void UpdateSource(WaveEntry source, long loopPoint, bool ignoreUpdatesUntilPlayed = false)
    {
        UpdateSource(source, -1, loopPoint, ignoreUpdatesUntilPlayed);
    }

    public void UpdateSource(WaveEntry source, bool ignoreUpdatesUntilPlayed = false)
    {
        UpdateSource(source, -1, -1, ignoreUpdatesUntilPlayed);
    }

    public void UpdateSource(WaveEntry source, double stopTime, double loopTime, bool ignoreUpdatesUntilPlayed = false)
    {
        UpdateSource(source, stopTime >= 0 ? WaveStreamUtil.TimeToBytePosition(source.Stream, stopTime) : -1, loopTime >= 0 ? WaveStreamUtil.TimeToBytePosition(source.Stream, loopTime) : -1, ignoreUpdatesUntilPlayed);
    }

    public void UpdateSource(WaveEntry source, double loopTime, bool ignoreUpdatesUntilPlayed = false)
    {
        UpdateSource(source, -1, loopTime, ignoreUpdatesUntilPlayed);
    }

    public void Deserialize(BinarySerializer serializer)
    {
        var soundName = serializer.ReadString();
        if (soundName != null)
        {
            source = GameEngine.Engine.soundStreams[soundName];
            Position = serializer.ReadLong();
        }
        else
            source = null;

        ignoreUpdatesUntilPlayed = serializer.ReadBool();
        StopPoint = serializer.ReadLong();
        LoopPoint = serializer.ReadLong();
        Playing = serializer.ReadBool();
    }

    public void Serialize(BinarySerializer serializer)
    {
        if (source != null)
        {
            serializer.WriteString(source.Name);
            serializer.WriteLong(Position);
        }
        else
            serializer.WriteString(null);

        serializer.WriteBool(ignoreUpdatesUntilPlayed);
        serializer.WriteLong(StopPoint);
        serializer.WriteLong(LoopPoint);
        serializer.WriteBool(Playing);
    }
}