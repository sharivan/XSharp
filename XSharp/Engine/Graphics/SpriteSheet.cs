using System;
using System.Collections.Generic;
using System.IO;

using SharpDX;
using SharpDX.Direct3D9;

using XSharp.Math;
using XSharp.Math.Geometry;

using Color = SharpDX.Color;
using MMXBox = XSharp.Math.Geometry.Box;
using Point = SharpDX.Point;

namespace XSharp.Engine.Graphics;

public class SpriteSheet : IDisposable
{
    public static GameEngine Engine => GameEngine.Engine;

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

        public Vector OriginOffset
        {
            get;
            set;
        }

        public MMXBox Hitbox
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

        public Frame AddFrame(FixedSingle originOffsetX, FixedSingle originOffsetY, int left, int top, int width, int height, int count = 1, bool loopPoint = false)
        {
            if (loopPoint)
                LoopFromSequenceIndex = frames.Count;

            var boundingBox = new MMXBox(left + originOffsetX + OriginOffset.X, top + originOffsetY + OriginOffset.Y, left, top, width, height);
            Frame frame = Sheet.AddFrame(boundingBox, Hitbox);
            AddRepeated(frame, count);
            return frame;
        }

        public Frame AddFrame(MMXBox boudingBox, MMXBox hitbox, int count = 1, bool loopPoint = false)
        {
            if (loopPoint)
                LoopFromSequenceIndex = frames.Count;

            Frame frame = Sheet.AddFrame(boudingBox, hitbox);
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

        public MMXBox Hitbox
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

        internal Frame(int index, MMXBox boundingBox, MMXBox hitbox, Texture texture, bool precached)
        {
            Index = index;
            BoundingBox = boundingBox;
            Hitbox = hitbox;
            Texture = texture;
            Precached = precached;
        }

        public override bool Equals(object obj)
        {
            return obj is Frame frame &&
                   EqualityComparer<MMXBox>.Default.Equals(BoundingBox, frame.BoundingBox) &&
                   EqualityComparer<MMXBox>.Default.Equals(Hitbox, frame.Hitbox) &&
                   EqualityComparer<Texture>.Default.Equals(Texture, frame.Texture);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(BoundingBox, Hitbox, Texture);
        }

        public override string ToString()
        {
            return "{" + BoundingBox + ", " + Hitbox + "}";
        }
    }

    internal string name;

    private readonly List<Frame> frames;
    private readonly Dictionary<string, FrameSequence> sequences;

    public int Index
    {
        get;
        internal set;
    }

    public string Name
    {
        get => name;
        set => GameEngine.Engine.UpdateSpriteSheetName(this, value);
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

    public Palette CurrentPalette
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

    internal SpriteSheet(bool disposeTexture = false, bool precache = false)
    {
        Precache = precache;
        DisposeTexture = disposeTexture;

        frames = new List<Frame>();
        sequences = new Dictionary<string, FrameSequence>();
    }

    internal SpriteSheet(Texture texture, bool disposeTexture = false, bool precache = false)
        : this(precache)
    {
        CurrentTexture = texture;
        DisposeTexture = disposeTexture;
    }

    internal SpriteSheet(string imageFileName, bool precache = false)
        : this(precache)
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

    public Frame AddFrame(MMXBox boudingBox, MMXBox hitbox)
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
                            int index = CurrentPalette.LookupColor(color);
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

            frame = new Frame(frames.Count, boudingBox - boudingBox.Origin, hitbox, texture, true);
            frames.Add(frame);
            return frame;
        }

        frame = new Frame(frames.Count, boudingBox - boudingBox.Origin, hitbox, CurrentTexture, false);
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
        if (sequences.TryGetValue(name, out FrameSequence value))
            return value;

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
        return sequences.TryGetValue(name, out FrameSequence value) ? value : null;
    }

    public FrameSequence RemoveFrameSequence(string name)
    {
        if (sequences.TryGetValue(name, out FrameSequence value))
        {
            FrameSequence result = value;
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
        GC.SuppressFinalize(this);
    }
}