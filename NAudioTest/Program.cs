using System.Diagnostics;

using SharpDX.Direct3D9;
using SharpDX.Windows;

using NAudio.Wave;

using Color = SharpDX.Color;

namespace NAudioTest
{
    internal static class Program
    {
        private const int SCREEN_WIDTH = 512;
        private const int SCREEN_HEIGHT = 448;
        private const int TICKRATE = 60;

        /// <summary>
        /// Stream for looping playback
        /// </summary>
        public class LoopStream : WaveStream
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
            }

            public LoopStream(WaveStream source, double stopTime, double loopPointTime) : this(source, stopTime >= 0 ? TimeToBytePosition(source, stopTime) : source.Length, loopPointTime >= 0 ? TimeToBytePosition(source, loopPointTime) : -1) { }

            public LoopStream(WaveStream sourceStream, long loopPoint) : this(sourceStream, -1, loopPoint) { }

            public LoopStream(WaveStream source, double loopPointTime) : this(source, -1, loopPointTime >= 0 ? TimeToBytePosition(source, loopPointTime) : -1) { }

            public LoopStream(WaveStream sourceStream) : this(sourceStream, -1, -1) { }

            /// <summary>
            /// Return source stream's wave format
            /// </summary>
            public override WaveFormat WaveFormat
            {
                get
                {
                    return SourceStream.WaveFormat;
                }
            }

            public WaveStream SourceStream
            {
                get;
                private set;
            }

            /// <summary>
            /// LoopStream simply returns
            /// </summary>
            public override long Length
            {
                get
                {
                    return SourceStream.Length;
                }
            }

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

            public override int Read(byte[] buffer, int offset, int count)
            {
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
        }

        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            var form = new Form()
            {
                Text = "NAudio Test",
                ClientSize = new System.Drawing.Size(SCREEN_WIDTH, SCREEN_HEIGHT)
            };

            var d3d = new Direct3D();
            var device = new Device(d3d, 0, DeviceType.Hardware, form.Handle, CreateFlags.HardwareVertexProcessing | CreateFlags.FpuPreserve | CreateFlags.Multithreaded, new PresentParameters(SCREEN_WIDTH, SCREEN_HEIGHT));

            #region Render loop

            // Create Clock and FPS counters
            var clock = new Stopwatch();
            double clockFrequency = Stopwatch.Frequency;
            clock.Start();
            var fpsTimer = new Stopwatch();
            fpsTimer.Start();
            double fps = 0.0;
            int fpsFrames = 0;

            double maxTimeToWait = 1000D / TICKRATE;

            var reader1 = new WaveFileReader(@"resources\sounds\mmx\04 - MMX - X Charge.wav");
            //var reader = new Mp3FileReader(@"resources\sounds\snd_player_charge");
            var loop1 = new LoopStream(reader1, 3.350, 1.585);
            //var loop = new LoopStream(reader, 4.450, 1.585);

            var player1 = new WaveOutEvent();

            player1.Init(loop1);
            player1.Volume = 0.25F;
            player1.Play();

            var reader2 = new WaveFileReader(@"resources\sounds\mmx\01 - MMX - X Regular Shot.wav");
            var loop2 = new LoopStream(reader2);

            var player2 = new WaveOutEvent();
            player2.Init(loop2);
            player2.Play();

            // Main loop
            RenderLoop.Run(form, () =>
            {
                // Time in seconds
                var totalSeconds = clock.ElapsedTicks / clockFrequency;

                #region FPS and title update
                fpsFrames++;
                if (fpsTimer.ElapsedMilliseconds > 1000)
                {
                    fps = 1000.0 * fpsFrames / fpsTimer.ElapsedMilliseconds;

                    // Update window title with FPS once every second
                    form.Text = string.Format("X# - FPS: {0:F2} ({1:F2}ms/frame)", fps, (float) fpsTimer.ElapsedMilliseconds / fpsFrames);

                    // Restart the FPS counter
                    fpsTimer.Reset();
                    fpsTimer.Start();
                    fpsFrames = 0;

                    loop2.Position = 0;
                }
                #endregion

                device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
                device.BeginScene();

                device.EndScene();
                device.Present();

                // Determine the time it took to render the frame
                double deltaTime = 1000 * (clock.ElapsedTicks / clockFrequency - totalSeconds);
                int delta = (int) (maxTimeToWait - deltaTime);
                if (delta > 0)
                    Thread.Sleep(delta);
            });

            #endregion

            device.Dispose();
            d3d.Dispose();
        }
    }
}