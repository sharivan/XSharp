using System;
using System.Collections.Generic;

using MMX.Geometry;
using MMX.Math;

using SharpDX;
using SharpDX.Direct3D9;

using MMXBox = MMX.Geometry.Box;

using static MMX.Engine.Consts;
using System.IO;

namespace MMX.Engine
{
    public class SpriteSheet : IDisposable
    {
        public class FrameSequence
        {
            private SpriteSheet sheet;
            private string name;
            private List<Frame> frames;
            private int loopFromSequenceIndex;
            private Vector boudingBoxOriginOffset;
            private MMXBox collisionBox;

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

            public Frame this[int index]
            {
                get
                {
                    return frames[index];
                }
            }

            public int Count
            {
                get
                {
                    return frames.Count;
                }
            }

            public int LoopFromSequenceIndex
            {
                get
                {
                    return loopFromSequenceIndex;
                }

                set
                {
                    loopFromSequenceIndex = value;
                }
            }

            public Vector BoudingBoxOriginOffset
            {
                get
                {
                    return boudingBoxOriginOffset;
                }

                set
                {
                    boudingBoxOriginOffset = value;
                }
            }

            public MMXBox CollisionBox
            {
                get
                {
                    return collisionBox;
                }

                set
                {
                    collisionBox = value;
                }
            }

            internal FrameSequence(SpriteSheet sheet, string name, int loopFromSequenceIndex = -1)
            {
                this.sheet = sheet;
                this.name = name;
                this.loopFromSequenceIndex = loopFromSequenceIndex;

                frames = new List<Frame>();
            }


            public void Add(Frame frame)
            {
                frames.Add(frame);
            }

            public void AddRepeated(Frame frame, int count)
            {
                for (int i = 0; i < count; i++)
                    Add(frame);
            }

            public void AddRange(Frame fromFrame, Frame toFrame)
            {
                for (int frameIndex = fromFrame.Index; frameIndex <= toFrame.Index; frameIndex++)
                    Add(sheet.GetFrame(frameIndex));
            }

            public void AddRangeRepeated(Frame fromFrame, Frame toFrame, int count)
            {
                for (int frameIndex = fromFrame.Index; frameIndex <= toFrame.Index; frameIndex++)
                    AddRepeated(sheet.GetFrame(frameIndex), count);
            }

            public void AddRangeRepeatedRange(Frame fromFrame, Frame toFrame, int count)
            {
                for (int i = 0; i < count; i++)
                    AddRange(fromFrame, toFrame);
            }

            public Frame AddFrame(int x, int y, int width, int height, int count = 1, bool loopPoint = false, OriginPosition originPosition = OriginPosition.CENTER)
            {
                if (loopPoint)
                    loopFromSequenceIndex = frames.Count;

                Frame frame = sheet.AddFrame(x, y, width, height, originPosition);
                AddRepeated(frame, count);
                return frame;
            }

            public Frame AddFrame(FixedSingle bbOriginXOff, FixedSingle bbOriginYOff, int bbLeft, int bbTop, int bbWidth, int bbHeight, int count = 1, bool loopPoint = false)
            {
                if (loopPoint)
                    loopFromSequenceIndex = frames.Count;

                MMXBox boundingBox = new MMXBox(bbLeft + bbOriginXOff + boudingBoxOriginOffset.X, bbTop + bbOriginYOff + boudingBoxOriginOffset.Y, bbLeft, bbTop, bbWidth, bbHeight);
                Frame frame = sheet.AddFrame(boundingBox, collisionBox);
                AddRepeated(frame, count);
                return frame;
            }

            public Frame AddFrame(MMXBox boudingBox, MMXBox collisionBox, int count = 1, bool loopPoint = false)
            {
                if (loopPoint)
                    loopFromSequenceIndex = frames.Count;

                Frame frame = sheet.AddFrame(boudingBox, collisionBox);
                AddRepeated(frame, count);
                return frame;
            }

            public void Clear()
            {
                frames.Clear();
            }

            public bool Remove(Frame frame)
            {
                return frames.Remove(frame);
            }

            public void Remove(int index)
            {
                frames.RemoveAt(index);
            }
        }

        public class Frame
        {
            private int index;
            private MMXBox boundingBox;
            private MMXBox collisionBox;
            private Texture bitmap;
            private bool precached;

            public int Index
            {
                get
                {
                    return index;
                }
            }

            public MMXBox BoundingBox
            {
                get
                {
                    return boundingBox;
                }
            }

            public MMXBox CollisionBox
            {
                get
                {
                    return collisionBox;
                }
            }

            public Texture Bitmap
            {
                get
                {
                    return bitmap;
                }
            }

            public bool Precached
            {
                get
                {
                    return precached;
                }
            }

            internal Frame(int index, MMXBox boundingBox, MMXBox collisionBox, Texture bitmap, bool precached)
            {
                this.index = index;
                this.boundingBox = boundingBox;
                this.collisionBox = collisionBox;
                this.bitmap = bitmap;
                this.precached = precached;
            }

            public override bool Equals(object obj)
            {
                var frame = obj as Frame;
                return frame != null &&
                       EqualityComparer<MMXBox>.Default.Equals(boundingBox, frame.boundingBox) &&
                       EqualityComparer<MMXBox>.Default.Equals(collisionBox, frame.collisionBox) &&
                       EqualityComparer<Texture>.Default.Equals(bitmap, frame.bitmap);
            }

            public override int GetHashCode()
            {
                var hashCode = -250932352;
                hashCode = hashCode * -1521134295 + EqualityComparer<MMXBox>.Default.GetHashCode(boundingBox);
                hashCode = hashCode * -1521134295 + EqualityComparer<MMXBox>.Default.GetHashCode(collisionBox);
                hashCode = hashCode * -1521134295 + EqualityComparer<Texture>.Default.GetHashCode(bitmap);
                return hashCode;
            }

            public override string ToString()
            {
                return "{" + boundingBox + ", " + collisionBox + "}";
            }
        }

        private GameEngine engine;
        private string name;
        private bool precache;

        private Texture currentBitmap;
        private Texture currentPalette;
        private bool disposeBitmap;

        private List<Frame> frames;
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

        public bool Precache
        {
            get
            {
                return precache;
            }

            set
            {
                precache = value;
            }
        }

        public Texture CurrentBitmap
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

        public Texture CurrentPalette
        {
            get
            {
                return currentPalette;
            }

            set
            {
                currentPalette = value;
            }
        }

        public bool DisposeBitmap
        {
            get
            {
                return disposeBitmap;
            }

            set
            {
                disposeBitmap = value;
            }
        }

        public void ReleaseCurrentBitmap()
        {
            if (disposeBitmap && currentBitmap != null)
                currentBitmap.Dispose();

            currentBitmap = null;
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

        public SpriteSheet(GameEngine engine, string name, bool disposeBitmap = false, bool precache = false)
        {
            this.engine = engine;
            this.name = name;
            this.precache = precache;
            this.disposeBitmap = disposeBitmap;

            frames = new List<Frame>();
            sequences = new Dictionary<string, FrameSequence>();
        }

        public SpriteSheet(GameEngine engine, string name, Texture bitmap, bool disposeBitmap = false, bool precache = false) :
            this(engine, name, precache)
        {
            currentBitmap = bitmap;
            this.disposeBitmap = disposeBitmap;
        }

        public SpriteSheet(GameEngine engine, string name, string imageFileName, bool precache = false) :
            this(engine, name, precache)
        {
            disposeBitmap = true;
            LoadFromFile(imageFileName);
        }

        public void LoadFromFile(string imageFileName)
        {
            currentBitmap = engine.CreateD2DBitmapFromFile(imageFileName);
        }

        public Frame AddFrame(int x, int y, int width, int height, OriginPosition originPosition = OriginPosition.CENTER)
        {
            MMXBox boudingBox = new MMXBox(x, y, width, height, originPosition);
            return AddFrame(boudingBox, boudingBox);
        }

        public Frame AddFrame(MMXBox boudingBox, MMXBox collisionBox)
        {
            Frame frame;

            if (precache)
            {
                var description = currentBitmap.GetLevelDescription(0);
                int srcWidth = description.Width;
                int srcHeight = description.Height;
                int srcX = (int) boudingBox.Left;
                int srcY = (int) boudingBox.Top;
                int width = (int) boudingBox.Width;
                int height = (int) boudingBox.Height;
                int width1 = (int) GameEngine.NextHighestPowerOfTwo((uint) width);
                int height1 = (int) GameEngine.NextHighestPowerOfTwo((uint) height);

                Texture texture;
                if (currentPalette == null)
                {
                    texture = new Texture(engine.Device, width1, height1, 1, Usage.None, description.Format, Pool.Default);

                    Surface src = currentBitmap.GetSurfaceLevel(0);
                    Surface dst = texture.GetSurfaceLevel(0);

                    engine.Device.UpdateSurface(src, GameEngine.ToRectangleF(boudingBox), dst, new Point(0, 0));
                }
                else
                {
                    texture = new Texture(engine.Device, width1, height1, 1, Usage.None, Format.L8, Pool.Managed);

                    DataRectangle srcRect = currentBitmap.LockRectangle(0, LockFlags.Discard);
                    DataRectangle dstRect = texture.LockRectangle(0, LockFlags.Discard);

                    using (DataStream dstDS = new DataStream(dstRect.DataPointer, width1 * height1 * sizeof(byte), true, true))
                    {                        
                        using (DataStream srcDS = new DataStream(srcRect.DataPointer, srcWidth * srcHeight * sizeof(int), true, true))
                        {
                            using (BinaryReader reader = new BinaryReader(srcDS))
                            {
                                for (int y = srcY; y < srcY + height; y++)
                                {
                                    for (int x = srcX; x < srcX + width; x++)
                                    {
                                        srcDS.Seek((y * srcRect.Pitch) + x * sizeof(int), SeekOrigin.Begin);
                                        int bgra = reader.ReadInt32();
                                        Color color = Color.FromBgra(bgra);
                                        int index = GameEngine.LookupColor(currentPalette, color);
                                        dstDS.Write((byte) (index != -1 ? index : 0));
                                    }

                                    for (int x = width; x < width1; x++)
                                        dstDS.Write((byte) 0);
                                }

                                for (int y = height; y < height1; y++)
                                {
                                    for (int x = 0; x < width1; x++)
                                        dstDS.Write((byte) 0);
                                }
                            }
                        }
                    }

                    /*using (DataStream dstDS = new DataStream(dstRect.DataPointer, width * height * sizeof(byte), true, true))
                    {
                        using (DataStream srcDS = new DataStream(srcRect.DataPointer, width * height * sizeof(int), true, true))
                        {
                            using (BinaryReader reader = new BinaryReader(srcDS))
                            {
                                for (int y = 0; y < width * height; y++)
                                {
                                    int bgra = reader.ReadInt32();
                                    Color color = Color.FromBgra(bgra);
                                    int index = GameEngine.LookupColor(currentPalette, color);
                                    dstDS.Write((byte) (index != -1 ? index : 0));
                                }
                            }
                        }
                    }*/

                    currentBitmap.UnlockRectangle(0);
                    texture.UnlockRectangle(0);
                }

                frame = new Frame(frames.Count, boudingBox - boudingBox.Origin, collisionBox, texture, true);
                frames.Add(frame);
                return frame;
            }

            frame = new Frame(frames.Count, boudingBox, collisionBox, currentBitmap, false);
            frames.Add(frame);
            return frame;
        }

        public Frame GetFrame(int frameIndex)
        {
            return frames[frameIndex];
        }

        public int IndexOfFrame(Frame frame)
        {
            return frames.IndexOf(frame);
        }

        public bool ContainsFrame(Frame frame)
        {
            return frames.Contains(frame);
        }

        public bool RemoveFrame(Frame frame)
        {
            return frames.Remove(frame);
        }

        public void RemoveFrame(int index)
        {
            frames.RemoveAt(index);
        }

        public void ClearFrames()
        {
            foreach (var frame in frames)
            {
                if (frame.Precached && frame.Bitmap != null)
                    frame.Bitmap.Dispose();
            }

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
            ClearFrames();
            ReleaseCurrentBitmap();
        }
    }
}
