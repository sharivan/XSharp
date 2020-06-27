using SharpDX.Direct2D1;
using System.Collections.Generic;

namespace MMX.Engine
{
    public class SpriteSheet
    {
        public class FrameSequence
        {
            private SpriteSheet sheet;
            private string name;
            private List<int> indices;
            private int loopFromFrame;
            private MMXVector offset;

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

            public MMXVector Offset
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

            public void AddFrame(MMXBox boudingBox, int count = 1)
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

        private SharpDX.Direct2D1.Bitmap d2dBitmap;
        private List<MMXBox> frames;
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

        public SharpDX.Direct2D1.Bitmap Image
        {
            get
            {
                return d2dBitmap;
            }

            set
            {
                d2dBitmap = value;
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

            frames = new List<MMXBox>();
            sequences = new Dictionary<string, FrameSequence>();
        }

        public SpriteSheet(GameEngine engine, string name, Bitmap image) :
            this(engine, name)
        {
            d2dBitmap = image;
        }

        public SpriteSheet(GameEngine engine, string name, string imageFileName) :
            this(engine, name)
        {
            LoadFromFile(imageFileName);
        }

        public void LoadFromFile(string imageFileName)
        {
            d2dBitmap = engine.CreateD2DBitmapFromFile(imageFileName);
        }

        public void AddFrame(int x, int y, int width, int height)
        {
            AddFrame(new MMXBox(x, y, x, y, width, height));
        }

        public void AddFrame(float cbX, float cbY, int bbX, int bbY, int bbWidth, int bbHeight)
        {
            AddFrame(new MMXBox(cbX, cbY, bbX, bbY, bbWidth, bbHeight));
        }

        public void AddFrame(MMXBox boudingBox)
        {
            frames.Add(boudingBox);
        }

        public MMXBox GetFrame(int frameIndex)
        {
            return frames[frameIndex];
        }

        public int IndexOfFrame(MMXBox frame)
        {
            return frames.IndexOf(frame);
        }

        public bool ContainsFrame(MMXBox frame)
        {
            return frames.Contains(frame);
        }

        public bool RemoveFrame(MMXBox frame)
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

        public void DrawFrame(int index)
        {
            DrawFrame(frames[index]);
        }

        public void DrawFrame(int index, MMXBox dstBox)
        {
            DrawFrame(dstBox, frames[index]);
        }

        public void DrawFrame(MMXBox box)
        {
            engine.Target.DrawBitmap(d2dBitmap, 1, BitmapInterpolationMode.NearestNeighbor, GameEngine.ToRectangleF(box));
        }

        public void DrawFrame(MMXBox dstBox, MMXBox box)
        {
            engine.Target.DrawBitmap(d2dBitmap, GameEngine.ToRectangleF(dstBox), 1, BitmapInterpolationMode.NearestNeighbor, GameEngine.ToRectangleF(box));
        }
    }
}
