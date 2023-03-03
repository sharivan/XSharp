using System;
using System.IO;

using NAudio.Wave;

namespace XSharp.Engine.Sound
{
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
    public class SoundStream : WaveStream
    {
        private WaveStream source;
        private bool ignoreUpdatesUntilPlayed;

        /// <summary>
        /// Return source stream's wave format
        /// </summary>
        public override WaveFormat WaveFormat => source.WaveFormat;

        public WaveStream Source
        {
            get => source;
            set => UpdateSource(value);
        }

        /// <summary>
        /// LoopStream simply returns
        /// </summary>
        public override long Length => source.Length;

        /// <summary>
        /// LoopStream simply passes on positioning to source stream
        /// </summary>
        public override long Position
        {
            get => source.Position;
            set => source.Position = value;
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

        public SoundStream(WaveStream source, long stopPoint, long loopPoint)
        {
            UpdateSource(source, stopPoint, loopPoint);
            Playing = true;
        }

        public SoundStream(WaveStream source, double stopTime, double loopTime) : this(source, stopTime >= 0 ? WaveStreamUtil.TimeToBytePosition(source, stopTime) : -1, loopTime >= 0 ? WaveStreamUtil.TimeToBytePosition(source, loopTime) : -1) { }

        public SoundStream(WaveStream source, long loopPoint) : this(source, -1, loopPoint) { }

        public SoundStream(WaveStream source) : this(source, -1, -1) { }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (!Playing)
                return 0;

            int totalBytesRead = 0;
            while (totalBytesRead < count)
            {
                int bytesToRead = count - totalBytesRead;
                if (source.Position + bytesToRead > StopPoint)
                    bytesToRead = (int) (StopPoint - source.Position);

                if (bytesToRead < 0)
                    bytesToRead = 0;

                int bytesRead = source.Read(buffer, offset + totalBytesRead, bytesToRead);
                if (bytesRead == 0)
                {
                    if (source.Position == 0 || !Looping)
                    {
                        // something wrong with the source stream
                        ignoreUpdatesUntilPlayed = false;
                        break;
                    }

                    // loop
                    source.Position = LoopPoint;
                }

                totalBytesRead += bytesRead;
            }

            return totalBytesRead;
        }

        public void Reset()
        {
            source.Position = 0;
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

        public void UpdateSource(WaveStream source, long stopPoint, long loopPoint, bool ignoreUpdatesUntilPlayed = false)
        {
            if (this.ignoreUpdatesUntilPlayed)
                return;

            this.source = source;
            StopPoint = stopPoint >= 0 ? stopPoint : source.Length;
            LoopPoint = loopPoint;
            this.ignoreUpdatesUntilPlayed = ignoreUpdatesUntilPlayed;
        }

        public void UpdateSource(WaveStream source, long loopPoint, bool ignoreUpdatesUntilPlayed = false)
        {
            UpdateSource(source, -1, loopPoint, ignoreUpdatesUntilPlayed);
        }

        public void UpdateSource(WaveStream source, bool ignoreUpdatesUntilPlayed = false)
        {
            UpdateSource(source, -1, -1, ignoreUpdatesUntilPlayed);
        }

        public void UpdateSource(WaveStream source, double stopTime, double loopTime, bool ignoreUpdatesUntilPlayed = false)
        {
            UpdateSource(source, stopTime >= 0 ? WaveStreamUtil.TimeToBytePosition(source, stopTime) : -1, loopTime >= 0 ? WaveStreamUtil.TimeToBytePosition(source, loopTime) : -1, ignoreUpdatesUntilPlayed);
        }

        public void UpdateSource(WaveStream source, double loopTime, bool ignoreUpdatesUntilPlayed = false)
        {
            UpdateSource(source, -1, loopTime, ignoreUpdatesUntilPlayed);
        }
    }
}
