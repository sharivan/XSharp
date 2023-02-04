using SharpDX;
using SharpDX.Direct3D9;
using System;
using System.Collections.Generic;
using System.IO;
using XSharp.Geometry;
using XSharp.Math;
using MMXBox = XSharp.Geometry.Box;

namespace XSharp.Engine.Graphics
{
    public class SpriteSheet : IDisposable
    {
        public class FrameSequence
        {
            private readonly List<Frame> frames;

            public SpriteSheet Sheet
            {
                get;
            }

            public string Name
            {
                get;
            }

            public Frame this[int index] => frames[index];

            public int Count => frames.Count;

            public int LoopFromSequenceIndex
            {
                get;
                set;
            }

            public Vector BoudingBoxOriginOffset
            {
                get;
                set;
            }

            public MMXBox CollisionBox
            {
                get;
                set;
            }

            internal FrameSequence(SpriteSheet sheet, string name, int loopFromSequenceIndex = -1)
            {
                Sheet = sheet;
                Name = name;
                LoopFromSequenceIndex = loopFromSequenceIndex;

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
                    Add(Sheet.GetFrame(frameIndex));
            }

            public void AddRangeRepeated(Frame fromFrame, Frame toFrame, int count)
            {
                for (int frameIndex = fromFrame.Index; frameIndex <= toFrame.Index; frameIndex++)
                    AddRepeated(Sheet.GetFrame(frameIndex), count);
            }

            public void AddRangeRepeatedRange(Frame fromFrame, Frame toFrame, int count)
            {
                for (int i = 0; i < count; i++)
                    AddRange(fromFrame, toFrame);
            }

            public Frame AddFrame(int left, int top, int width, int height, int count = 1, bool loopPoint = false, OriginPosition originPosition = OriginPosition.CENTER)
            {
                if (loopPoint)
                    LoopFromSequenceIndex = frames.Count;

                Frame frame = Sheet.AddFrame(left, top, width, height, originPosition);
                AddRepeated(frame, count);
                return frame;
            }

            public Frame AddFrame(FixedSingle bbOriginXOff, FixedSingle bbOriginYOff, int bbLeft, int bbTop, int bbWidth, int bbHeight, int count = 1, bool loopPoint = false)
            {
                if (loopPoint)
                    LoopFromSequenceIndex = frames.Count;

                var boundingBox = new MMXBox(bbLeft + bbOriginXOff + BoudingBoxOriginOffset.X, bbTop + bbOriginYOff + BoudingBoxOriginOffset.Y, bbLeft, bbTop, bbWidth, bbHeight);
                Frame frame = Sheet.AddFrame(boundingBox, CollisionBox);
                AddRepeated(frame, count);
                return frame;
            }

            public Frame AddFrame(MMXBox boudingBox, MMXBox collisionBox, int count = 1, bool loopPoint = false)
            {
                if (loopPoint)
                    LoopFromSequenceIndex = frames.Count;

                Frame frame = Sheet.AddFrame(boudingBox, collisionBox);
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
            public int Index
            {
                get;
            }

            public MMXBox BoundingBox
            {
                get;
            }

            public MMXBox CollisionBox
            {
                get;
            }

            public Texture Texture
            {
                get;
            }

            public bool Precached
            {
                get;
            }

            internal Frame(int index, MMXBox boundingBox, MMXBox collisionBox, Texture texture, bool precached)
            {
                Index = index;
                BoundingBox = boundingBox;
                CollisionBox = collisionBox;
                Texture = texture;
                Precached = precached;
            }

            public override bool Equals(object obj)
            {
                return obj is Frame frame &&
                       EqualityComparer<MMXBox>.Default.Equals(BoundingBox, frame.BoundingBox) &&
                       EqualityComparer<MMXBox>.Default.Equals(CollisionBox, frame.CollisionBox) &&
                       EqualityComparer<Texture>.Default.Equals(Texture, frame.Texture);
            }

            public override int GetHashCode()
            {
                var hashCode = -250932352;
                hashCode = hashCode * -1521134295 + EqualityComparer<MMXBox>.Default.GetHashCode(BoundingBox);
                hashCode = hashCode * -1521134295 + EqualityComparer<MMXBox>.Default.GetHashCode(CollisionBox);
                hashCode = hashCode * -1521134295 + EqualityComparer<Texture>.Default.GetHashCode(Texture);
                return hashCode;
            }

            public override string ToString()
            {
                return "{" + BoundingBox + ", " + CollisionBox + "}";
            }
        }

        private readonly List<Frame> frames;
        private readonly Dictionary<string, FrameSequence> sequences;

        public GameEngine Engine => GameEngine.Engine;

        public int Index
        {
            get;
            internal set;
        }

        public string Name
        {
            get;
        }

        public bool Precache
        {
            get;
            set;
        }

        public Texture CurrentTexture
        {
            get;
            set;
        }

        public Texture CurrentPalette
        {
            get;
            set;
        }

        public bool DisposeTexture
        {
            get;
            set;
        }

        public IEnumerable<string> FrameSequenceNames => sequences.Keys;

        public IEnumerable<FrameSequence> FrameSequences => sequences.Values;

        public int FrameCount => frames.Count;

        public int FrameSequenceCount => sequences.Count;

        public SpriteSheet(string name, bool disposeTexture = false, bool precache = false)
        {
            Name = name;
            Precache = precache;
            DisposeTexture = disposeTexture;

            frames = new List<Frame>();
            sequences = new Dictionary<string, FrameSequence>();
        }

        public SpriteSheet(string name, Texture texture, bool disposeTexture = false, bool precache = false)
            : this(name, precache)
        {
            CurrentTexture = texture;
            DisposeTexture = disposeTexture;
        }

        public SpriteSheet(string name, string imageFileName, bool precache = false)
            : this(name, precache)
        {
            DisposeTexture = true;
            LoadFromFile(imageFileName);
        }

        public void LoadFromFile(string imageFileName)
        {
            CurrentTexture = Engine.CreateImageTextureFromFile(imageFileName);
        }

        public void ReleaseCurrentTexture()
        {
            if (DisposeTexture && CurrentTexture != null)
                CurrentTexture.Dispose();

            CurrentTexture = null;
        }

        public Frame AddFrame(int left, int top, int width, int height, OriginPosition originPosition = OriginPosition.CENTER)
        {
            var boudingBox = new MMXBox(left, top, width, height, originPosition);
            return AddFrame(boudingBox, boudingBox - boudingBox.Origin);
        }

        public Frame AddFrame(MMXBox boudingBox, MMXBox collisionBox)
        {
            Frame frame;

            if (Precache)
            {
                var description = CurrentTexture.GetLevelDescription(0);
                int srcWidth = description.Width;
                int srcHeight = description.Height;
                int srcX = (int) boudingBox.Left;
                int srcY = (int) boudingBox.Top;
                int width = (int) boudingBox.Width;
                int height = (int) boudingBox.Height;
                int width1 = (int) GameEngine.NextHighestPowerOfTwo((uint) width);
                int height1 = (int) GameEngine.NextHighestPowerOfTwo((uint) height);

                Texture texture;
                if (CurrentPalette == null)
                {
                    texture = new Texture(Engine.Device, width1, height1, 1, Usage.None, description.Format, Pool.Default);

                    Surface src = CurrentTexture.GetSurfaceLevel(0);
                    Surface dst = texture.GetSurfaceLevel(0);

                    Engine.Device.UpdateSurface(src, GameEngine.ToRectangleF(boudingBox), dst, new Point(0, 0));
                }
                else
                {
                    texture = new Texture(Engine.Device, width1, height1, 1, Usage.None, Format.L8, Pool.Managed);

                    DataRectangle srcRect = CurrentTexture.LockRectangle(0, LockFlags.Discard);
                    DataRectangle dstRect = texture.LockRectangle(0, LockFlags.Discard);

                    using (var dstDS = new DataStream(dstRect.DataPointer, width1 * height1 * sizeof(byte), true, true))
                    {
                        using var srcDS = new DataStream(srcRect.DataPointer, srcWidth * srcHeight * sizeof(int), true, true);
                        using var reader = new BinaryReader(srcDS);
                        for (int y = srcY; y < srcY + height; y++)
                        {
                            for (int x = srcX; x < srcX + width; x++)
                            {
                                srcDS.Seek(y * srcRect.Pitch + x * sizeof(int), SeekOrigin.Begin);
                                int bgra = reader.ReadInt32();
                                var color = Color.FromBgra(bgra);
                                int index = GameEngine.LookupColor(CurrentPalette, color);
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

                    CurrentTexture.UnlockRectangle(0);
                    texture.UnlockRectangle(0);
                }

                frame = new Frame(frames.Count, boudingBox - boudingBox.Origin, collisionBox, texture, true);
                frames.Add(frame);
                return frame;
            }

            frame = new Frame(frames.Count, boudingBox - boudingBox.Origin, collisionBox, CurrentTexture, false);
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
                if (frame.Precached && frame.Texture != null)
                    frame.Texture.Dispose();
            }

            frames.Clear();
        }

        public FrameSequence AddFrameSquence(string name, int loopFromFrame = -1)
        {
            if (sequences.ContainsKey(name))
                return sequences[name];

            var result = new FrameSequence(this, name, loopFromFrame);
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
            return sequences.ContainsKey(name) ? sequences[name] : null;
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
            ReleaseCurrentTexture();
        }
    }
}
