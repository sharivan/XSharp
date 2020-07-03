using MMX.Geometry;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.DXGI;
using System;
using System.Collections.Generic;

using static MMX.Engine.Consts;

namespace MMX.Engine
{
    public class SpriteSheet : IDisposable
    {
        public class FrameSequence
        {
            private SpriteSheet sheet;
            private string name;
            private List<int> indices;
            private int loopFromFrame;
            private Vector offset;

            public SpriteSheet Sheet
            {
                get
                {
                    return sheet;
                }
            }

            public string Name
            {
                get
                {
                    return name;
                }
            }

            public int this[int index]
            {
                get
                {
                    return indices[index];
                }
            }

            public int Count
            {
                get
                {
                    return indices.Count;
                }
            }

            public int LoopFromFrame
            {
                get
                {
                    return loopFromFrame;
                }
            }

            public Vector Offset
            {
                get
                {
                    return offset;
                }

                set
                {
                    offset = value;
                }
            }

            internal FrameSequence(SpriteSheet sheet, string name, int loopFromFrame = -1)
            {
                this.sheet = sheet;
                this.name = name;
                this.loopFromFrame = loopFromFrame;

                indices = new List<int>();
            }


            public void Add(int frameIndex)
            {
                indices.Add(frameIndex);
            }

            public void AddRepeated(int frameIndex, int count)
            {
                for (int i = 0; i < count; i++)
                    Add(frameIndex);
            }

            public void AddRange(int startFrameIndex, int endFrameIndex)
            {
                for (int frameIndex = startFrameIndex; frameIndex <= endFrameIndex; frameIndex++)
                    Add(frameIndex);
            }

            public void AddRangeRepeated(int startFrameIndex, int endFrameIndex, int count)
            {
                for (int frameIndex = startFrameIndex; frameIndex <= endFrameIndex; frameIndex++)
                    AddRepeated(frameIndex, count);
            }

            public void AddRangeRepeatedRange(int startFrameIndex, int endFrameIndex, int count)
            {
                for (int i = 0; i < count; i++)
                    AddRange(startFrameIndex, endFrameIndex);
            }

            public void AddFrame(int x, int y, int width, int height, int count = 1)
            {
                sheet.AddFrame((int) (x + offset.X), (int) (y + offset.Y), x, y, width, height);
                AddRepeated(sheet.FrameCount - 1, count);
            }

            public void AddFrame(float cbXOff, float cbYOff, int bbX, int bbY, int bbWidth, int bbHeight, int count = 1)
            {
                sheet.AddFrame((int) (bbX + cbXOff + offset.X), (int) (bbY + cbYOff + offset.Y), bbX, bbY, bbWidth, bbHeight);
                AddRepeated(sheet.FrameCount - 1, count);
            }

            public void AddFrame(Box boudingBox, int count = 1)
            {
                sheet.AddFrame(boudingBox + offset);
                AddRepeated(sheet.FrameCount - 1, count);
            }

            public void Clear()
            {
                indices.Clear();
            }

            public void Remove(int index)
            {
                indices.RemoveAt(index);
            }
        }

        private GameEngine engine;
        private string name;

        private Bitmap currentBitmap;
        private List<Tuple<Box, Bitmap>> frames;
        private Dictionary<string, FrameSequence> sequences;

        public GameEngine Engine
        {
            get
            {
                return engine;
            }
        }

        public string Name
        {
            get
            {
                return name;
            }
        }

        public Bitmap CurrentBitmap
        {
            get
            {
                return currentBitmap;
            }

            set
            {
                currentBitmap = value;
            }
        }

        public void ReleaseCurrentBitmap()
        {
            if (currentBitmap != null)
            {
                currentBitmap.Dispose();
                currentBitmap = null;
            }
        }

        public int FrameCount
        {
            get
            {
                return frames.Count;
            }
        }

        public int FrameSequenceCount
        {
            get
            {
                return sequences.Count;
            }
        }

        public SpriteSheet(GameEngine engine, string name)
        {
            this.engine = engine;
            this.name = name;

            frames = new List<Tuple<Box, Bitmap>>();
            sequences = new Dictionary<string, FrameSequence>();
        }

        public SpriteSheet(GameEngine engine, string name, Bitmap image) :
            this(engine, name)
        {
            currentBitmap = image;
        }

        public SpriteSheet(GameEngine engine, string name, string imageFileName) :
            this(engine, name)
        {
            LoadFromFile(imageFileName);
        }

        public void LoadFromFile(string imageFileName)
        {
            currentBitmap = engine.CreateD2DBitmapFromFile(imageFileName);
        }

        public void AddFrame(int x, int y, int width, int height)
        {
            AddFrame(new Box(x, y, x, y, width, height));
        }

        public void AddFrame(float cbX, float cbY, int bbX, int bbY, int bbWidth, int bbHeight)
        {
            AddFrame(new Box(cbX, cbY, bbX, bbY, bbWidth, bbHeight));
        }

        public void AddFrame(Box boudingBox)
        {
            var size = new Size2((int) boudingBox.Width, (int) boudingBox.Height);
            var sizef = new Size2F((float) boudingBox.Width, (float) boudingBox.Height);
            var pixelFormat = new PixelFormat(Format.B8G8R8A8_UNorm, SharpDX.Direct2D1.AlphaMode.Premultiplied);
            var properties = new BitmapProperties(pixelFormat);

            using (BitmapRenderTarget target = new BitmapRenderTarget(engine.Target, CompatibleRenderTargetOptions.None, sizef, size, pixelFormat))
            {
                target.AntialiasMode = ANTIALIAS_MODE;

                target.BeginDraw();
                target.DrawBitmap(currentBitmap, new RectangleF(0, 0, sizef.Width, sizef.Height), 1, INTERPOLATION_MODE, GameEngine.ToRectangleF(boudingBox));
                target.Flush();
                target.EndDraw();

                Tuple<Box, Bitmap> frame = new Tuple<Box, Bitmap>(boudingBox - boudingBox.Origin, target.Bitmap);
                frames.Add(frame);
            } 
        }

        public Tuple<Box, Bitmap> GetFrame(int frameIndex)
        {
            return frames[frameIndex];
        }

        public int IndexOfFrame(Tuple<Box, Bitmap> frame)
        {
            return frames.IndexOf(frame);
        }

        public bool ContainsFrame(Tuple<Box, Bitmap> frame)
        {
            return frames.Contains(frame);
        }

        public bool RemoveFrame(Tuple<Box, Bitmap> frame)
        {
            return frames.Remove(frame);
        }

        public void RemoveFrame(int index)
        {
            frames.RemoveAt(index);
        }

        public void ClearFrames()
        {
            frames.Clear();
        }

        public FrameSequence AddFrameSquence(string name, int loopFromFrame = -1)
        {
            if (sequences.ContainsKey(name))
                return sequences[name];

            FrameSequence result = new FrameSequence(this, name, loopFromFrame);
            sequences[name] = result;
            return result;
        }

        public Dictionary<string, FrameSequence>.Enumerator GetFrameSequenceEnumerator()
        {
            return sequences.GetEnumerator();
        }

        public bool ContainsFrameSequence(string name)
        {
            return sequences.ContainsKey(name);
        }

        public FrameSequence GetFrameSequence(string name)
        {
            if (sequences.ContainsKey(name))
                return sequences[name];

            return null;
        }

        public FrameSequence RemoveFrameSequence(string name)
        {
            if (sequences.ContainsKey(name))
            {
                FrameSequence result = sequences[name];
                sequences.Remove(name);
                return result;
            }

            return null;
        }

        public void ClearFrameSequences()
        {
            sequences.Clear();
        }

        public void Dispose()
        {
            ReleaseCurrentBitmap();

            foreach (var frame in frames)
                frame.Item2.Dispose();
        }
    }
}
