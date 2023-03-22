using XSharp.Engine.Graphics;
using XSharp.Math;
using XSharp.Math.Geometry;

using static XSharp.Engine.Entities.Enemies.Bosses.Penguin.Penguin;

namespace XSharp.Engine.Entities.Enemies.Bosses.Penguin;

internal class PenguinSnow : Enemy
{
    #region Precache
    [Precache]
    new internal static void Precache()
    {
        Engine.CallPrecacheAction(typeof(Penguin));
    }
    #endregion

    private int frameCounter;

    public Penguin Shooter
    {
        get;
        internal set;
    }

    public PenguinSnow()
    {
        Layer = 1;
        Directional = true;
        SpriteSheetName = "Penguin";
        ContactDamage = 0;

        SetAnimationNames("Snow");
    }

    protected override Box GetHitbox()
    {
        return PENGUIN_SNOW_HITBOX;
    }

    public override FixedSingle GetGravity()
    {
        return 0;
    }

    protected override bool OnTakeDamage(Sprite attacker, ref FixedSingle damage)
    {
        return false;
    }

    protected override void OnSpawn()
    {
        base.OnSpawn();

        CheckCollisionWithWorld = false;
        Direction = Shooter.Direction;
        Origin = Shooter.Origin + (Shooter.Direction == Shooter.DefaultDirection ? -PENGUIN_SHOT_ORIGIN_OFFSET.X : PENGUIN_SHOT_ORIGIN_OFFSET.X, PENGUIN_SHOT_ORIGIN_OFFSET.Y);
        Invincible = true;

        frameCounter = 0;

        SetCurrentAnimationByName("Snow");
    }

    protected override void OnThink()
    {
        base.OnThink();

        Velocity = (Direction == Direction.RIGHT ? PENGUIN_SNOW_SPEED : -PENGUIN_SNOW_SPEED, 0);

        frameCounter++;
        if (frameCounter >= PENGUIN_SNOW_FRAMES)
            Kill();
    }

    protected override void OnStartTouch(Entity entity)
    {
        base.OnStartTouch(entity);

        var player = Engine.Player;
        if (entity == player)
            Shooter.FreezePlayer();
    }
}