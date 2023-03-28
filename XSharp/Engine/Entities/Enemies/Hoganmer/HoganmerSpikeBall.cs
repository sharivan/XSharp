using XSharp.Engine.Graphics;
using XSharp.Math;
using XSharp.Math.Geometry;

namespace XSharp.Engine.Entities.Enemies.Hoganmer;

public class HoganmerSpikeBall : Enemy
{
    #region Precache
    [Precache]
    new internal static void Precache()
    {
        Engine.CallPrecacheAction<Hoganmer>();
    }
    #endregion

    private Vector desaceleration;
    private FixedSingle distance;
    private bool stoped;
    private int stoppingFrames;
    private bool backing;
    private Vector backingInitialSpeed;

    private AnimationReference spikeBallAnimation;
    private AnimationReference[] chainAnimations;

    public Vector ThrowOrigin
    {
        get;
        internal set;
    }

    public Hoganmer Thrower => Parent as Hoganmer;

    public HoganmerSpikeBall()
    {
    }

    protected override void OnCreate()
    {
        base.OnCreate();

        chainAnimations = new AnimationReference[5];

        SpawnFacedToPlayer = false;
        HitResponse = HitResponse.REFLECT;
        DefaultDirection = Direction.RIGHT;
        ContactDamage = Hoganmer.SPIKE_BALL_DAMAGE;

        PaletteName = "HoganmerPalette";
        SpriteSheetName = "Hoganmer";
        MultiAnimation = true;

        SetAnimationNames(("SpikeBallChain", chainAnimations.Length, true, 0), ("SpikeBall", 1, false, 1));
    }

    public override FixedSingle GetGravity()
    {
        return 0;
    }

    protected override Box GetHitbox()
    {
        return Hoganmer.SPIKE_BALL_HITBOX;
    }

    protected override void OnSpawn()
    {
        base.OnSpawn();

        CheckCollisionWithWorld = false;

        stoped = false;
        backing = false;

        for (int i = 0; i < chainAnimations.Length; i++)
        {
            chainAnimations[i] = GetAnimationByName("SpikeBallChain" + i);
            chainAnimations[i].Target.Visible = true;
        }

        spikeBallAnimation = GetAnimationByName("SpikeBall");
        spikeBallAnimation.Target.Visible = true;
    }

    protected override void OnPostSpawn()
    {
        base.OnPostSpawn();

        ThrowOrigin = Origin;
        var velocityVersor = Velocity.Versor();
        desaceleration = (Hoganmer.SPIKE_BALL_DESACELERATION * velocityVersor).TruncFracPart();
        distance = 0;
    }

    protected override void OnThink()
    {
        base.OnThink();

        if (!stoped)
        {
            if (backing)
            {
                Velocity += desaceleration;
                distance -= Velocity.Length.TruncFracPart();

                if (distance < 0)
                {
                    Velocity = Vector.NULL_VECTOR;
                    Thrower?.NotifySpikeBallBack();
                    Kill();
                }
            }
            else
            {
                Velocity -= desaceleration;
                distance += Velocity.Length.TruncFracPart();

                if (distance >= Hoganmer.SPIKE_BALL_MAX_DISTANCE)
                {
                    backingInitialSpeed = Velocity;
                    Velocity = Vector.NULL_VECTOR;
                    stoped = true;
                    stoppingFrames = 0;
                }
            }
        }
        else
        {
            Velocity = Vector.NULL_VECTOR;
            stoppingFrames++;
            if (stoppingFrames >= Hoganmer.SPIKE_BALL_STOP_FRAMES_BEFORE_BACK)
            {
                Velocity = -backingInitialSpeed;
                desaceleration = -desaceleration;
                stoped = false;
                backing = true;
            }
        }

        var chainOffset = (Origin - ThrowOrigin) / (chainAnimations.Length + 1);
        for (int i = 0; i < chainAnimations.Length; i++)
        {
            var chainAnimation = chainAnimations[i];
            chainAnimation.Target.Offset = -(i + 1) * chainOffset;
        }
    }
}