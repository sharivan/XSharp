using System;
using System.Collections.Generic;
using System.IO;

using XSharp.Graphics;
using XSharp.Math;
using XSharp.Math.Geometry;

namespace XSharp.Engine.Graphics;

public abstract class SpriteSheet : IDisposable
{
    public static BaseEngine Engine => BaseEngine.Engine;

    protected static Frame CreateFrame(int index, Box boundingBox, Box hitbox, ITexture texture, bool precached)
    {
        return new Frame(index, boundingBox, hitbox, texture, precached);
    }

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

        public int LoopFromFrame
        {
            get;
            set;
        }

        public Vector OriginOffset
        {
            get;
            set;
        }

        public Box Hitbox
        {
            get;
            set;
        }

        internal FrameSequence(SpriteSheet sheet, string name, int loopFromFrame = -1)
        {
            Sheet = sheet;
            Name = name;
            LoopFromFrame = loopFromFrame;

            frames = [];
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
                LoopFromFrame = frames.Count;

            Frame frame = Sheet.AddFrame(left, top, width, height, originPosition);
            AddRepeated(frame, count);
            return frame;
        }

        public Frame AddFrame(FixedSingle originOffsetX, FixedSingle originOffsetY, int left, int top, int width, int height, int count = 1, bool loopPoint = false)
        {
            if (loopPoint)
                LoopFromFrame = frames.Count;

            var boundingBox = new Box(left + originOffsetX + OriginOffset.X, top + originOffsetY + OriginOffset.Y, left, top, width, height);
            Frame frame = Sheet.AddFrame(boundingBox, Hitbox);
            AddRepeated(frame, count);
            return frame;
        }

        public Frame AddFrame(Box boudingBox, Box hitbox, int count = 1, bool loopPoint = false)
        {
            if (loopPoint)
                LoopFromFrame = frames.Count;

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

        public Box BoundingBox
        {
            get;
        }

        public Box Hitbox
        {
            get;
        }

        public ITexture Texture
        {
            get;
        }

        public bool Precached
        {
            get;
        }

        internal Frame(int index, Box boundingBox, Box hitbox, ITexture texture, bool precached)
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
                   EqualityComparer<Box>.Default.Equals(BoundingBox, frame.BoundingBox) &&
                   EqualityComparer<Box>.Default.Equals(Hitbox, frame.Hitbox) &&
                   EqualityComparer<ITexture>.Default.Equals(Texture, frame.Texture);
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

    protected readonly List<Frame> frames;
    protected readonly Dictionary<string, FrameSequence> sequences;

    public int Index
    {
        get;
        internal set;
    }

    public string Name
    {
        get => name;
        set => BaseEngine.Engine.UpdateSpriteSheetName(this, value);
    }

    public bool Precache
    {
        get;
        set;
    }

    public ITexture CurrentTexture
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

    protected SpriteSheet(bool disposeTexture = false, bool precache = false)
    {
        Precache = precache;
        DisposeTexture = disposeTexture;

        frames = [];
        sequences = [];
    }

    protected SpriteSheet(ITexture texture, bool disposeTexture = false, bool precache = false)
        : this(precache)
    {
        CurrentTexture = texture;
        DisposeTexture = disposeTexture;
    }

    protected SpriteSheet(string imageFileName, bool precache = false)
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
        var boudingBox = new Box(left, top, width, height, originPosition);
        return AddFrame(boudingBox, boudingBox - boudingBox.Origin);
    }

    public abstract Frame AddFrame(Box boudingBox, Box hitbox);

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