using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using System.Diagnostics;

using SharpDX;
using SharpDX.Direct3D9;
using SharpDX.DirectSound;
using SharpDX.Windows;

using DSLockFlags = SharpDX.DirectSound.LockFlags;
using NAudio.Wave;

namespace DSoundText
{
    public class Program
    {
        private const int SCREEN_WIDTH = 512;
        private const int SCREEN_HEIGHT = 448;
        private const int TICKRATE = 60;

        [STAThread]
        public static void Main(string[] args)
        {
            var form = new Form()
            {
                Text = "SharpDX - DirectSound Test",
                ClientSize = new System.Drawing.Size(SCREEN_WIDTH, SCREEN_HEIGHT)
            };

            var d3d = new Direct3D();
            var device = new Device(d3d, 0, DeviceType.Hardware, form.Handle, CreateFlags.HardwareVertexProcessing | CreateFlags.FpuPreserve | CreateFlags.Multithreaded, new PresentParameters(SCREEN_WIDTH, SCREEN_HEIGHT));

            var ds = new DirectSound();

            // Set Cooperative Level to PRIORITY (priority level can call the SetFormat and Compact methods)
            //
            ds.SetCooperativeLevel(form.Handle, CooperativeLevel.Priority);

            // Create PrimarySoundBuffer
            var primaryBufferDesc = new SoundBufferDescription
            {
                Flags = BufferFlags.PrimaryBuffer,
                AlgorithmFor3D = Guid.Empty
            };

            var primarySoundBuffer = new PrimarySoundBuffer(ds, primaryBufferDesc);
            //primarySoundBuffer.Format = new WaveFormat(32000, 16, 1);

            // Play the PrimarySound Buffer
            primarySoundBuffer.Play(0, PlayFlags.Looping);

            Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("DSoundText.resources.sounds.mmx.05 - MMX - X Charge + Shot.wav");
            //PlaySoundAsync(ds, stream);

            var reader = new WaveFileReader(stream);
            byte[] buffer = new byte[reader.Length];
            int read = reader.Read(buffer, 0, buffer.Length);
            short[] sampleBuffer = new short[read / 2];
            Buffer.BlockCopy(buffer, 0, sampleBuffer, 0, read);

            var format = new SharpDX.Multimedia.WaveFormat(reader.WaveFormat.SampleRate, reader.WaveFormat.BitsPerSample, reader.WaveFormat.Channels);

            var desc2 = new SoundBufferDescription
            {
                Format = format,
                Flags = BufferFlags.GlobalFocus,
                //BufferBytes = format.AverageBytesPerSecond
                BufferBytes = buffer.Length
            };

            var sBuffer1 = new SecondarySoundBuffer(ds, desc2);
            byte[] bytes = new byte[desc2.BufferBytes];

            //sBuffer1.Write(buffer, 0, desc2.BufferBytes, 0, DSLockFlags.None);
            sBuffer1.Write(buffer, 0, DSLockFlags.None);
            sBuffer1.Play(0, PlayFlags.None);

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

            sBuffer1.Play(0, PlayFlags.None);

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

            stream.Close();
            stream.Dispose();
            reader.Dispose();

            device.Dispose();
            d3d.Dispose();
        }

        private static void PlaySoundAsync(DirectSound ds, string audioFile)
        {
            using (Stream stream = File.Open(audioFile, FileMode.Open))
            {
                PlaySoundAsync(ds, stream);
            }
        }

        private static void PlaySoundAsync(DirectSound ds, Stream stream)
        {
            var format = new SharpDX.Multimedia.WaveFormat(32000, 16, 1);

            var desc = new SoundBufferDescription
            {
                Format = format,
                Flags = BufferFlags.GlobalFocus,
                BufferBytes = 8 * format.AverageBytesPerSecond
            };

            var pBuffer = new PrimarySoundBuffer(ds, desc);

            var desc2 = new SoundBufferDescription
            {
                Format = format,
                Flags = BufferFlags.GlobalFocus | BufferFlags.ControlPositionNotify | BufferFlags.GetCurrentPosition2,
                BufferBytes = 8 * format.AverageBytesPerSecond
            };

            var sBuffer1 = new SecondarySoundBuffer(ds, desc2);

            var notifications = new NotificationPosition[2];
            notifications[0] = new NotificationPosition();
            notifications[1] = new NotificationPosition();

            notifications[0].Offset = desc2.BufferBytes / 2 + 1;
            notifications[1].Offset = desc2.BufferBytes - 1;

            notifications[0].WaitHandle = new AutoResetEvent(false);
            notifications[1].WaitHandle = new AutoResetEvent(false);
            sBuffer1.SetNotificationPositions(notifications);

            byte[] bytes1 = new byte[desc2.BufferBytes / 2];
            byte[] bytes2 = new byte[desc2.BufferBytes];

            var fillBuffer = new Thread(() => {
                //int readNumber = 1;
                int bytesRead;

                bytesRead = stream.Read(bytes2, 0, desc2.BufferBytes);
                sBuffer1.Write(bytes2, 0, DSLockFlags.None);
                sBuffer1.Play(0, PlayFlags.None);

                while (true)
                {
                    if (bytesRead == 0)
                        break;

                    notifications[0].WaitHandle.WaitOne();
                    bytesRead = stream.Read(bytes1, 0, bytes1.Length);
                    sBuffer1.Write(bytes1, 0, DSLockFlags.None);

                    if (bytesRead == 0)
                        break;

                    notifications[1].WaitHandle.WaitOne();
                    bytesRead = stream.Read(bytes1, 0, bytes1.Length);
                    sBuffer1.Write(bytes1, desc2.BufferBytes / 2, DSLockFlags.None);
                }

                stream.Close();
                stream.Dispose();
            });

            fillBuffer.Start();
        }
    }
}
