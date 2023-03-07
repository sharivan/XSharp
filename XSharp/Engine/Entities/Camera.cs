using System.IO;

using XSharp.Engine.Entities.Items;
using XSharp.Math;
using XSharp.Math.Geometry;

using static XSharp.Engine.Consts;

using MMXWorld = XSharp.Engine.World.World;

namespace XSharp.Engine.Entities;

public class Camera : Entity
{
    private EntityReference focusOn = null;
    private Vector moveDistance;

    public bool NoConstraints
    {
        get;
        set;
    }

    public FixedSingle Width
    {
        get => Size.X;
        set => Size = (value, Height);
    }

    public FixedSingle Height
    {
        get => Size.Y;
        set => Size = (Width, value);
    }

    public Vector Size
    {
        get;
        set;
    }

    public static MMXWorld World => GameEngine.Engine.World;

    public FixedSingle SmoothSpeed
    {
        get;
        set;
    } = CAMERA_SMOOTH_SPEED;

    public Vector Velocity
    {
        get;
        private set;
    } = Vector.NULL_VECTOR;

    public Vector LeftTop
    {
        get => Center - Size * FixedSingle.HALF;
        set => Center = value + Size * FixedSingle.HALF;
    }

    public FixedSingle Left
    {
        get => LeftTop.X;
        set => LeftTop = (value, LeftTop.Y);
    }

    public FixedSingle Top
    {
        get => LeftTop.Y;
        set => LeftTop = (LeftTop.X, value);
    }

    public Vector LeftMiddle
    {
        get => (Center.X - Width * FixedSingle.HALF, Center.Y);
        set => Center = (value.X + Width * FixedSingle.HALF, value.Y);
    }

    public Vector LeftBottom
    {
        get => (Center.X - Width * FixedSingle.HALF, Center.Y + Height * FixedSingle.HALF);
        set => Center = (value.X + Width * FixedSingle.HALF, value.Y - Height * FixedSingle.HALF);
    }

    public Vector MiddleTop
    {
        get => (Center.X, Center.Y - Height * FixedSingle.HALF);
        set => Center = (value.X, value.Y + Height * FixedSingle.HALF);
    }

    public Vector Center
    {
        get => (Origin.X, Origin.Y - (Width - Height) * FixedSingle.HALF);
        set => Origin = (value.X, value.Y + (Width - Height) * FixedSingle.HALF);
    }

    new public Vector Origin
    {
        get => base.Origin;
        set
        {
            if (FocusOn != null)
                return;

            SetOrigin(value);
        }
    }

    public Vector MiddleBottom
    {
        get => (Center.X, Center.Y + Height * FixedSingle.HALF);
        set => Center = (value.X, value.Y - Height * FixedSingle.HALF);
    }

    public Vector RightTop
    {
        get => (Center.X + Width * FixedSingle.HALF, Center.Y - Height * FixedSingle.HALF);
        set => Center = (value.X - Width * FixedSingle.HALF, value.Y + Height * FixedSingle.HALF);
    }

    public Vector RightMiddle
    {
        get => (Center.X + Width * FixedSingle.HALF, Center.Y);
        set => Center = (value.X - Width * FixedSingle.HALF, value.Y);
    }

    public Vector RightBottom
    {
        get => Center + Size * FixedSingle.HALF;
        set => Center = value - Size * FixedSingle.HALF;
    }

    public FixedSingle Right
    {
        get => RightBottom.X;
        set => RightBottom = (value, RightBottom.Y);
    }

    public FixedSingle Bottom
    {
        get => RightBottom.Y;
        set => RightBottom = (RightBottom.X, value);
    }

    public Entity FocusOn
    {
        get => focusOn;
        set
        {
            focusOn = value;
            if (FocusOn != null)
                MoveToFocus();
        }
    }

    public Box BoundingBox
    {
        get
        {
            Vector sv2 = Size * FixedSingle.HALF;
            return new Box(Center, -sv2, sv2);
        }
    }

    public Box LiveBoundingBox
    {
        get
        {
            Vector sv2 = Size * FixedSingle.HALF + EXTENDED_BORDER_LIVE_SCREEN_OFFSET;
            return new Box(Center, -sv2, sv2);
        }
    }

    public Box SpawnBoundingBox
    {
        get
        {
            Vector sv2 = Size * FixedSingle.HALF + EXTENDED_BORDER_SPAWN_SCREEN_OFFSET;
            return new Box(Center, -sv2, sv2);
        }
    }

    public bool Moving => moveDistance.X >= STEP_SIZE || moveDistance.Y >= STEP_SIZE;

    public Camera()
    {
        TouchingKind = TouchingKind.VECTOR;
        Respawnable = true;
    }

    public override void LoadState(BinaryReader reader)
    {
        base.LoadState(reader);

        int focusedObjectIndex = reader.ReadInt32();
        focusOn = focusedObjectIndex >= 0 ? Engine.entities[focusedObjectIndex] : null;
        moveDistance = new Vector(reader);
        NoConstraints = reader.ReadBoolean();
        Size = new Vector(reader);
        SmoothSpeed = new FixedSingle(reader);
        Velocity = new Vector(reader);
    }

    public override void SaveState(BinaryWriter writer)
    {
        base.SaveState(writer);

        writer.Write(FocusOn != null ? FocusOn.Index : -1);
        moveDistance.Write(writer);
        writer.Write(NoConstraints);
        Size.Write(writer);
        SmoothSpeed.Write(writer);
        Velocity.Write(writer);
    }

    protected override Box GetHitbox()
    {
        var box = SpawnBoundingBox;
        return box - Origin;
    }

    private void SetOrigin(Vector v, bool clamp = true)
    {
        SetOrigin(v.X, v.Y, clamp);
    }

    public Vector ClampToBounds(Vector origin)
    {
        return ClampToBounds(origin.X, origin.Y);
    }

    public Vector ClampToBounds(FixedSingle x, FixedSingle y)
    {
        y -= (Width - Height) * FixedSingle.HALF;

        Vector minCameraPos = Engine.MinCameraPos;
        Vector maxCameraPos = Engine.MaxCameraPos;

        FixedSingle w2 = Width * 0.5;
        FixedSingle h2 = Height * 0.5;

        FixedSingle minX = minCameraPos.X + w2;
        FixedSingle minY = minCameraPos.Y + h2;
        FixedSingle maxX = FixedSingle.Min(maxCameraPos.X, World.Width) - w2;
        FixedSingle maxY = FixedSingle.Min(maxCameraPos.Y, World.Height) - h2;

        if (x < minX)
            x = minX;
        else if (x > maxX)
            x = maxX;

        if (y < minY)
            y = minY;
        else if (y > maxY)
            y = maxY;

        return (x, y + (Width - Height) * FixedSingle.HALF);
    }

    private void SetOrigin(FixedSingle x, FixedSingle y, bool clamp = true)
    {
        base.Origin = (clamp ? ClampToBounds(x, y) : new Vector(x, y)).TruncFracPart();
    }

    private Vector GetSmoothVelocity()
    {
        return Engine.Player != null && FocusOn == Engine.Player && Engine.Player.NoClip
            ? (NO_CLIP_SPEED_BOOST, NO_CLIP_SPEED_BOOST)
            : (SmoothSpeed, SmoothSpeed);
    }

    public void MoveToLeftTop(Vector dest)
    {
        MoveToOrigin(dest + new Vector(Width, Width) * FixedSingle.HALF, GetSmoothVelocity());
    }

    public void MoveToLeftTop(Vector dest, Vector velocity)
    {
        MoveToOrigin(dest + new Vector(Width, Width) * FixedSingle.HALF, velocity);
    }

    public void MoveToOrigin(Vector dest)
    {
        MoveToOrigin(dest, GetSmoothVelocity());
    }

    public void MoveToOrigin(Vector origin, Vector velocity)
    {
        focusOn = null;
        MoveToOriginInternal(origin, velocity);
    }

    private bool MoveToOriginInternal(Vector origin, Vector velocity)
    {
        if (velocity.X < STEP_SIZE && velocity.Y < STEP_SIZE)
            return false;

        var moveTo = ClampToBounds(origin.RoundToFloor());
        var dx = moveTo.X - Origin.X;
        var dy = moveTo.Y - Origin.Y;
        Vector moveDistance = (dx.Abs, dy.Abs);

        bool moveX = true;
        bool moveY = true;

        if (moveDistance.X < STEP_SIZE)
        {
            moveX = false;
            moveDistance = moveDistance.YVector;
            velocity = velocity.YVector;
        }

        if (moveDistance.Y < STEP_SIZE)
        {
            moveY = false;
            moveDistance = moveDistance.XVector;
            velocity = velocity.XVector;
        }

        if (!moveX && !moveY)
            return false;

        Velocity = (velocity.X.TruncFracPart() * dx.Signal, velocity.Y.TruncFracPart() * dy.Signal);
        this.moveDistance = moveDistance;
        return true;
    }

    private void MoveToFocus(Vector velocity)
    {
        if (FocusOn == null)
            return;

        Vector dest = FocusOn.Origin;
        MoveToOriginInternal(dest, velocity);
    }

    private void MoveToFocus()
    {
        MoveToFocus(GetSmoothVelocity());
    }

    public void StopMoving()
    {
        moveDistance = Vector.NULL_VECTOR;
    }

    public Box VisibleBox(Box box)
    {
        return BoundingBox & box;
    }

    public bool IsVisible(Box box)
    {
        return VisibleBox(box).IsValid();
    }

    protected internal override void OnSpawn()
    {
        base.OnSpawn();

        CheckTouchingEntities = true;
        CheckTouchingWithDeadEntities = true;
    }

    private bool DoMoving()
    {
        if (FocusOn != null)
            MoveToFocus();

        Vector velocity = Velocity;
        moveDistance -= (velocity.X.Abs, velocity.Y.Abs);

        if (moveDistance.X < STEP_SIZE)
        {
            Velocity = Velocity.YVector;
            velocity = ((moveDistance.X + velocity.X.Abs) * velocity.X.Signal, velocity.Y);
            moveDistance = moveDistance.YVector;
        }

        if (moveDistance.Y < STEP_SIZE)
        {
            Velocity = Velocity.XVector;
            velocity = (velocity.X, (moveDistance.Y + velocity.Y.Abs) * velocity.Y.Signal);
            moveDistance = moveDistance.XVector;
        }

        if (velocity != Vector.NULL_VECTOR)
        {
            Vector oldOrigin = Origin;
            Vector newOrigin = oldOrigin + velocity;
            SetOrigin(newOrigin, false);

            if ((Origin.X - oldOrigin.X).Abs < STEP_SIZE)
            {
                Velocity = Velocity.YVector;
                moveDistance = moveDistance.YVector;
            }

            if ((Origin.Y - oldOrigin.Y).Abs < STEP_SIZE)
            {
                Velocity = Velocity.XVector;
                moveDistance = moveDistance.XVector;
            }

            return true;
        }

        return false;
    }

    // TODO : Smooth movement is not working right for some situations. Please fix it!
    protected internal override void PostThink()
    {
        base.PostThink();

        DoMoving();
    }

    protected override void OnStartTouch(Entity entity)
    {
        base.OnStartTouch(entity);

        if (entity.Alive || entity.Spawning || !entity.Respawnable || !entity.RespawnOnNear)
            return;

        if ((!entity.Dead || Engine.FrameCounter - entity.DeathFrame >= entity.MinimumIntervalToRespawn) && entity.IsInSpawnArea(VectorKind.ORIGIN))
        {
            // TODO : This needs a special check for Heart Tanks and Sub-Tanks
            if (entity is Item item && item.Collected)
                return;

            if (entity is Sprite sprite)
                sprite.Visible = true;

            entity.Spawn();
        }
    }
}