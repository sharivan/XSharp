using System;
using System.IO;

using NAudio.Wave;

namespace MMX.Engine.Sound
{
    public class SoundPlayer : IDisposable
    {
        /// <summary>
        /// Stream for looping playback
        /// </summary>
        private class LoopStream : WaveStream
        {
            public static long TimeToBytePosition(WaveStream stream, double time) => (long) (time * stream.WaveFormat.AverageBytesPerSecond);

            /// <summary>
            /// Creates a new Loop stream
            /// </summary>
            /// <param name="sourceStream">The stream to read from. Note: the Read method of this stream should return 0 when it reaches the end
            /// or else we will not loop to the start again.</param>
            public LoopStream(WaveStream sourceStream, long stopPoint, long loopPoint)
            {
                SourceStream = sourceStream;
                StopPoint = stopPoint < 0 ? sourceStream.Length : stopPoint;
                LoopPoint = loopPoint;

                Playing = true;
            }

            public LoopStream(WaveStream source, double stopTime, double loopPointTime) : this(source, stopTime >= 0 ? TimeToBytePosition(source, stopTime) : source.Length, loopPointTime >= 0 ? TimeToBytePosition(source, loopPointTime) : -1) { }

            public LoopStream(WaveStream sourceStream, long loopPoint) : this(sourceStream, -1, loopPoint) { }

            public LoopStream(WaveStream source, double loopPointTime) : this(source, -1, loopPointTime >= 0 ? TimeToBytePosition(source, loopPointTime) : -1) { }

            public LoopStream(WaveStream sourceStream) : this(sourceStream, -1, -1) { }

            /// <summary>
            /// Return source stream's wave format
            /// </summary>
            public override WaveFormat WaveFormat => SourceStream.WaveFormat;

            public WaveStream SourceStream
            {
                get;
                private set;
            }

            /// <summary>
            /// LoopStream simply returns
            /// </summary>
            public override long Length => SourceStream.Length;

            /// <summary>
            /// LoopStream simply passes on positioning to source stream
            /// </summary>
            public override long Position
            {
                get => SourceStream.Position;
                set => SourceStream.Position = value;
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

            public override int Read(byte[] buffer, int offset, int count)
            {
                if (!Playing)
                    return 0;

                int totalBytesRead = 0;
                while (totalBytesRead < count)
                {
                    int bytesToRead = count - totalBytesRead;
                    if (SourceStream.Position + bytesToRead > StopPoint)
                        bytesToRead = (int) (StopPoint - SourceStream.Position);

                    int bytesRead = SourceStream.Read(buffer, offset + totalBytesRead, bytesToRead);
                    if (bytesRead == 0)
                    {
                        if (SourceStream.Position == 0 || !Looping)
                        {
                            // something wrong with the source stream
                            break;
                        }

                        // loop
                        SourceStream.Position = LoopPoint;
                    }

                    totalBytesRead += bytesRead;
                }

                return totalBytesRead;
            }

            public void Reset()
            {
                SourceStream.Position = 0;
            }

            public new void Dispose()
            {
                base.Dispose();
                SourceStream.Dispose();
            }

            public void Play() => Playing = true;

            public void Stop() => Playing = false;
        }

        public enum SoundFormat
        {
            WAVE,
            MP3
        }

        private LoopStream loop;
        private WaveOutEvent player;

        public SoundFormat Format
        {
            get;
            private set;
        }
        public float Volume
        {
            get => player.Volume;
            set => player.Volume = value;
        }

        private void Initialize(WaveStream stream, double stopTime, double loopTime)
        {        
            loop = new LoopStream(stream, stopTime, loopTime);

            player = new WaveOutEvent();
            player.Init(loop);
        }


        public SoundPlayer(Stream stream, SoundFormat format, double stopTime, double loopTime)
        {
            Format = format;

            WaveStream reader = format switch
            {
                SoundFormat.WAVE => new WaveFileReader(stream),
                SoundFormat.MP3 => new Mp3FileReader(stream),
                _ => throw new ArgumentException($"Sound format is invalid: {format}"),
            };

            Initialize(reader, stopTime, loopTime);
        }

        public SoundPlayer(Stream stream, SoundFormat format) : this(stream, format, -1, -1) { }

        public SoundPlayer(Stream stream, SoundFormat format, double loopTime) : this(stream, format, -1, loopTime) { }

        public SoundPlayer(string fileName, SoundFormat format, double stopTime, double loopTime)
        {
            Format = format;

            WaveStream reader = format switch
            {
                SoundFormat.WAVE => new WaveFileReader(fileName),
                SoundFormat.MP3 => new Mp3FileReader(fileName),
                _ => throw new ArgumentException($"Sound format is invalid: {format}"),
            };

            Initialize(reader, stopTime, loopTime);
        }

        public SoundPlayer(string fileName, SoundFormat format) : this(fileName, format, -1, -1) { }

        public SoundPlayer(string fileName, SoundFormat format, double loopTime) : this(fileName, format, -1, loopTime) { }

        public void Play()
        {
            loop.Reset();
            loop.Play();
            player.Play();
        }

        public void Stop()
        {
            loop.Stop();
            player.Stop();
        }

        public void Dispose()
        {
            player.Dispose();
            loop.Dispose();
        }
    }
}
