using XSharp.Engine.Entities;
using XSharp.Factories;
using XSharp.Graphics;
using XSharp.Math.Fixed;
using XSharp.Math.Fixed.Geometry;
using XSharp.Math.Geometry;
using XSharp.Serialization;

using MMXBox = XSharp.Math.Fixed.Geometry.Box;
using Sprite = XSharp.Engine.Entities.Sprite;

namespace XSharp.Engine.Graphics;

public delegate void AnimationFrameEvent(Animation animation, int frame);

[Serializable]
public class Animation : IIndexedNamedFactoryItem, IRenderable
{
    internal EntityReference<Sprite> sprite;
    internal string name;
    private bool animating;
    private int currentFrame;
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

    [field: NotSerializable]
    private SpriteSheet.FrameSequence Sequence
    {
        get
        {
            field ??= SpriteSheet.GetFrameSequence(FrameSequenceName);
            return field;
        }

        set;
    }

    public SpriteSheet SpriteSheet => Sprite.SpriteSheet;

    public int InitialFrame
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
        get => currentFrame;

        set
        {
            currentFrame = value >= Sequence.Count ? Sequence.Count - 1 : value;
            animationEndFired = false;
        }
    }

    public MMXBox CurrentFrameBoundingBox
                => CurrentFrame < 0 || CurrentFrame > Sequence.Count
                ? MMXBox.EMPTY_BOX
                : Sequence[CurrentFrame].BoundingBox;

    public MMXBox CurrentFrameHitbox
                => CurrentFrame < 0 || CurrentFrame > Sequence.Count
                ? MMXBox.EMPTY_BOX
                : Sequence[CurrentFrame].Hitbox;

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

    public int LoopFromFrame => Sequence.LoopFromFrame;

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

    public Vector DrawOrigin => Sprite.PixelOrigin + Offset;

    public MMXBox DrawBox
    {
        get
        {
            var frame = Sequence[CurrentFrame];
            var drawOrigin = DrawOrigin;
            var box = drawOrigin + frame.BoundingBox;

            FixedSingle drawScale = Entity.Engine.DrawScale;
            if (drawScale != 1)
                box.Scale(drawOrigin, drawScale);

            if (Mirrored || Sprite.MirrorAnimationFromDirection && Sprite.Direction != Sprite.DefaultDirection)
                box = box.Mirror(drawOrigin);

            if (Flipped || Sprite.UpsideDown)
                box = box.Flip(drawOrigin);

            FixedSingle rotation = Rotation + Sprite.Rotation.ToRadians();
            if (rotation != FixedSingle.ZERO)
            {
                var ltRotatedDiff = box.LeftTop.Rotate(drawOrigin, rotation) - drawOrigin;
                var rbRotatedDiff = box.RightBottom.Rotate(drawOrigin, rotation) - drawOrigin;
                Vector mins = (FixedSingle.Min(ltRotatedDiff.X, rbRotatedDiff.X), FixedSingle.Min(ltRotatedDiff.Y, rbRotatedDiff.Y));
                Vector maxs = (FixedSingle.Max(ltRotatedDiff.X, rbRotatedDiff.X), FixedSingle.Max(ltRotatedDiff.Y, rbRotatedDiff.Y));
                box = (drawOrigin, mins, maxs);
            }

            if (Scale != FixedSingle.ONE)
                box = box.Scale(drawOrigin, Scale);

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

        Sequence = SpriteSheet.GetFrameSequence(frameSequenceName);
        InitialFrame = initialFrame;
        Visible = startVisible;
        animating = startOn;
        Mirrored = mirrored;
        Flipped = flipped;
        Rotation = rotation;

        Scale = 1;

        currentFrame = initialFrame;
        animationEndFired = false;

        int count = Sequence.Count;
        animationEvents = new AnimationFrameEvent[count];
    }

    public void SetEvent(int frame, AnimationFrameEvent animationEvent)
    {
        animationEvents[frame] = animationEvent;
    }

    public void ClearEvent(int frame)
    {
        SetEvent(frame, null);
    }

    public void Start(int startIndex = -1)
    {
        animationEndFired = false;
        animating = true;

        if (startIndex >= 0)
            CurrentFrame = InitialFrame + startIndex;
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
        CurrentFrame = InitialFrame;
    }

    internal void NextFrame()
    {
        // Se a animação não está em execução ou não ouver pelo menos dois quadros na animação então não é necessário computar o próximo quadro da animação
        if (!animating || animationEndFired || Sequence.Count == 0)
            return;

        animationEvents[CurrentFrame]?.Invoke(this, CurrentFrame);
        currentFrame++;

        if (currentFrame >= Sequence.Count)
        {
            currentFrame = Sequence.Count - 1;

            if (!animationEndFired)
            {
                animationEndFired = true;

                if (Sprite.MultiAnimation || Sprite.CurrentAnimation == this)
                    Sprite.NotifyAnimationEnd(this);
            }

            if (Sequence.LoopFromFrame != -1) // e se a animação está em looping, então o próximo frame deverá ser o primeiro frame da animação (não o frame inicial, definido por initialFrame)
            {
                currentFrame = Sequence.LoopFromFrame;
                animationEndFired = false;
            }
        }
    }

    public void Render(IRenderTarget target)
    {
        // Se não estiver visível ou não ouver frames então não precisa desenhar nada
        if (!BaseEngine.Engine.Editing && !Visible || Sequence.Count == 0 || RepeatX <= 0 || RepeatY <= 0)
            return;

        var frame = Sequence[CurrentFrame];
        MMXBox srcBox = frame.BoundingBox;
        ITexture texture = frame.Texture;

        Vector origin = DrawOrigin;
        MMXBox drawBox = origin + srcBox;
        Vector2 translatedOrigin = BaseEngine.Engine.WorldVectorToScreen(drawBox.Origin, true);
        var origin3 = new Vector3(translatedOrigin.X, translatedOrigin.Y, 0);

        Matrix transform = Matrix.Identity;

        float drawScale = (float) BaseEngine.Engine.DrawScale;
        transform *= Matrix.Scaling(drawScale);

        if (Mirrored || Sprite.MirrorAnimationFromDirection && Sprite.Direction != Sprite.DefaultDirection)
            transform *= Matrix.Translation(-origin3) * Matrix.Scaling(-1, 1, 1) * Matrix.Translation(origin3);

        if (Flipped || Sprite.UpsideDown)
            transform *= Matrix.Translation(-origin3) * Matrix.Scaling(1, -1, 1) * Matrix.Translation(origin3);

        FixedSingle rotation = Rotation + Sprite.Rotation.ToRadians();
        if (rotation != FixedSingle.ZERO)
            transform *= Matrix.Translation(-origin3) * Matrix.RotationZ((float) rotation) * Matrix.Translation(origin3);

        if (Scale != FixedSingle.ONE)
            transform *= Matrix.Translation(-origin3) * Matrix.Scaling((float) Scale) * Matrix.Translation(origin3);

        BaseEngine.Engine.RenderSprite(texture, Sprite.Palette, Sprite.FadingControl, drawBox.LeftTop, transform, RepeatX, RepeatY);
    }

    internal void OnDeviceReset()
    {
        Sequence = null;
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