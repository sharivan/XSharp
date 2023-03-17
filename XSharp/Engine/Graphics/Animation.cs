﻿using SharpDX;
using SharpDX.Direct3D9;

using XSharp.Engine.Entities;
using XSharp.Factories;
using XSharp.Math;
using XSharp.Math.Geometry;
using XSharp.Serialization;

using MMXBox = XSharp.Math.Geometry.Box;
using Sprite = XSharp.Engine.Entities.Sprite;

namespace XSharp.Engine.Graphics;

public delegate void AnimationFrameEvent(Animation animation, int frameSequenceIndex);

[Serializable]
public class Animation : IIndexedNamedFactoryItem, IRenderable
{
    internal EntityReference<Sprite> sprite;
    internal string name;

    [NotSerializable]
    private SpriteSheet.FrameSequence sequence;

    private bool animating;
    private int currentSequenceIndex;
    private bool animationEndFired;

    private AnimationFrameEvent[] animationEvents;

    public Sprite Sprite => sprite;

    public IIndexedNamedFactory Factory => Sprite.Animations;

    public int Index
    {
        get;
        internal set;
    }

    public string Name
    {
        get => name;
        set => Sprite.Animations.UpdateAnimationName(this, value);
    }

    public string FrameSequenceName
    {
        get;
        private set;
    }

    private SpriteSheet.FrameSequence Sequence
    {
        get
        {
            sequence ??= SpriteSheet.GetFrameSequence(FrameSequenceName);
            return sequence;
        }
    }

    public SpriteSheet SpriteSheet => Sprite.SpriteSheet;

    public int InitialSequenceIndex
    {
        get;
        set;
    }

    public Vector Offset
    {
        get;
        set;
    } = Vector.NULL_VECTOR;

    public int CurrentFrame
    {
        get => currentSequenceIndex;

        set
        {
            currentSequenceIndex = value >= Sequence.Count ? Sequence.Count - 1 : value;
            animationEndFired = false;
        }
    }

    public MMXBox CurrentFrameBoundingBox
                => currentSequenceIndex < 0 || currentSequenceIndex > Sequence.Count
                ? MMXBox.EMPTY_BOX
                : Sequence[currentSequenceIndex].BoundingBox;

    public MMXBox CurrentFrameHitbox
                => currentSequenceIndex < 0 || currentSequenceIndex > Sequence.Count
                ? MMXBox.EMPTY_BOX
                : Sequence[currentSequenceIndex].Hitbox;

    public bool Visible
    {
        get;
        set;
    }

    public bool Animating
    {
        get => animating;

        set
        {
            if (value && !animating)
                Start();
            else if (!value && animating)
                Stop();
        }
    }

    public int LoopFromFrame => Sequence.LoopFromSequenceIndex;

    public FixedSingle Rotation
    {
        get;
        set;
    }

    public FixedSingle Scale
    {
        get;
        set;
    }

    public bool Flipped
    {
        get;
        set;
    }

    public bool Mirrored
    {
        get;
        set;
    }

    public int RepeatX
    {
        get;
        set;
    } = 1;

    public int RepeatY
    {
        get;
        set;
    } = 1;

    public Vector DrawOrigin => Sprite.IntegerOrigin + Offset;

    public MMXBox DrawBox
    {
        get
        {
            var frame = Sequence[currentSequenceIndex];
            var drawOrigin = DrawOrigin;
            var box = drawOrigin + frame.BoundingBox;

            FixedSingle drawScale = Entity.Engine.DrawScale;
            if (drawScale != 1)
                box.Scale(drawOrigin, drawScale);

            if (Rotation != FixedSingle.ZERO)
            {
                var ltRotatedDiff = box.LeftTop.Rotate(drawOrigin, Rotation) - drawOrigin;
                var rbRotatedDiff = box.RightBottom.Rotate(drawOrigin, Rotation) - drawOrigin;
                Vector mins = (FixedSingle.Min(ltRotatedDiff.X, rbRotatedDiff.X), FixedSingle.Min(ltRotatedDiff.Y, rbRotatedDiff.Y));
                Vector maxs = (FixedSingle.Max(ltRotatedDiff.X, rbRotatedDiff.X), FixedSingle.Max(ltRotatedDiff.Y, rbRotatedDiff.Y));
                box = (drawOrigin, mins, maxs);
            }

            if (Scale != FixedSingle.ONE)
                box = box.Scale(drawOrigin, Scale);

            if (Flipped)
                box = box.Flip(drawOrigin);

            if (Mirrored || Sprite.Directional && Sprite.Direction != Sprite.DefaultDirection)
                box = box.Mirror(drawOrigin);

            return box;
        }
    }

    public Animation()
    {
    }

    internal void Initialize(string frameSequenceName, Vector offset, int repeatX, int repeatY, int initialFrame = 0, bool startVisible = true, bool startOn = true, bool mirrored = false, bool flipped = false)
    {
        Initialize(frameSequenceName, offset, FixedSingle.ZERO, repeatX, repeatY, initialFrame, startVisible, startOn, mirrored, flipped);
    }

    internal void Initialize(string frameSequenceName, Vector offset, FixedSingle rotation, int repeatX, int repeatY, int initialFrame = 0, bool startVisible = true, bool startOn = true, bool mirrored = false, bool flipped = false)
    {
        FrameSequenceName = frameSequenceName;
        Offset = offset;
        RepeatX = repeatX;
        RepeatY = repeatY;

        sequence = SpriteSheet.GetFrameSequence(frameSequenceName);
        InitialSequenceIndex = initialFrame;
        Visible = startVisible;
        animating = startOn;
        Mirrored = mirrored;
        Flipped = flipped;
        Rotation = rotation;

        Scale = 1;

        currentSequenceIndex = initialFrame;
        animationEndFired = false;

        int count = Sequence.Count;
        animationEvents = new AnimationFrameEvent[count];
    }

    public void SetEvent(int frameSequenceIndex, AnimationFrameEvent animationEvent)
    {
        animationEvents[frameSequenceIndex] = animationEvent;
    }

    public void ClearEvent(int frameSequenceIndex)
    {
        SetEvent(frameSequenceIndex, null);
    }

    public void Start(int startIndex = -1)
    {
        animationEndFired = false;
        animating = true;

        if (startIndex >= 0)
            currentSequenceIndex = InitialSequenceIndex + startIndex;
    }

    public void StartFromBegin()
    {
        Start(0);
    }

    public void Stop()
    {
        animating = false;
    }

    public void Reset()
    {
        currentSequenceIndex = InitialSequenceIndex;
    }

    internal void NextFrame()
    {
        // Se a animação não está em execução ou não ouver pelo menos dois quadros na animação então não é necessário computar o próximo quadro da animação
        if (!animating || animationEndFired || Sequence.Count == 0)
            return;

        animationEvents[currentSequenceIndex]?.Invoke(this, currentSequenceIndex);
        currentSequenceIndex++;

        if (currentSequenceIndex >= Sequence.Count)
        {
            currentSequenceIndex = Sequence.Count - 1;

            if (!animationEndFired)
            {
                animationEndFired = true;

                if (Sprite.MultiAnimation || Sprite.CurrentAnimation == this)
                    Sprite.OnAnimationEnd(this);
            }

            if (Sequence.LoopFromSequenceIndex != -1) // e se a animação está em looping, então o próximo frame deverá ser o primeiro frame da animação (não o frame inicial, definido por initialFrame)
            {
                currentSequenceIndex = Sequence.LoopFromSequenceIndex;
                animationEndFired = false;
            }
        }
    }

    public void Render(IRenderTarget target)
    {
        // Se não estiver visível ou não ouver frames então não precisa desenhar nada
        if (!Visible || Sequence.Count == 0 || RepeatX <= 0 || RepeatY <= 0)
            return;

        var frame = Sequence[currentSequenceIndex];
        MMXBox srcBox = frame.BoundingBox;
        Texture texture = frame.Texture;

        Vector origin = DrawOrigin;
        MMXBox drawBox = origin + srcBox;
        Vector2 translatedOrigin = GameEngine.Engine.WorldVectorToScreen(drawBox.Origin);
        var origin3 = new Vector3(translatedOrigin.X, translatedOrigin.Y, 0);

        Matrix transform = Matrix.Identity;

        float drawScale = (float) GameEngine.Engine.DrawScale;
        transform *= Matrix.Scaling(drawScale);

        if (Rotation != FixedSingle.ZERO)
            transform *= Matrix.Translation(-origin3) * Matrix.RotationZ((float) Rotation) * Matrix.Translation(origin3);

        if (Scale != FixedSingle.ONE)
            transform *= Matrix.Translation(-origin3) * Matrix.Scaling((float) Scale) * Matrix.Translation(origin3);

        if (Flipped)
        {
            if (Mirrored || Sprite.Directional && Sprite.Direction != Sprite.DefaultDirection)
                transform *= Matrix.Translation(-origin3) * Matrix.Scaling(-1, -1, 1) * Matrix.Translation(origin3);
            else
                transform *= Matrix.Translation(-origin3) * Matrix.Scaling(1, -1, 1) * Matrix.Translation(origin3);
        }
        else if (Mirrored || Sprite.Directional && Sprite.Direction != Sprite.DefaultDirection)
        {
            transform *= Matrix.Translation(-origin3) * Matrix.Scaling(-1, 1, 1) * Matrix.Translation(origin3);
        }

        GameEngine.Engine.RenderSprite(texture, Sprite.Palette, Sprite.FadingControl, drawBox.LeftTop, transform, RepeatX, RepeatY);
    }

    internal void OnDeviceReset()
    {
        sequence = null;
    }

    public override int GetHashCode()
    {
        return Index;
    }

    public override string ToString()
    {
        return Sequence.Name + (Rotation != 0 ? " rotation " + Rotation : "") + (Scale != 0 ? " scale " + Scale : "") + (Mirrored ? " mirrored" : "") + (Flipped ? " flipped" : "");
    }
}