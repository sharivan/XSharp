using XSharp.Engine.Graphics;
using XSharp.Math.Geometry;

namespace XSharp.Engine.Entities.Enemies.BombBeen;

public enum BombBeenBombState
{
    DROPPED = 0,
    PLANTED = 1,
    IDLE = 2,
    ABOUT_TO_EXPLODE = 3
}

public class BombBeenBomb : Sprite, IStateEntity<BombBeenBombState>
{
    #region Precache
    [Precache]
    internal static void Precache()
    {
        Engine.CallPrecacheAction(typeof(BombBeen));
    }
    #endregion

    private int tick;

    public BombBeenBombState State
    {
        get => GetState<BombBeenBombState>();
        set => SetState(value);
    }

    public int FramesToExplode
    {
        get;
        set;
    } = BombBeen.BOMB_FRAMES_TO_EXPLODE;

    public BombBeenBomb()
    {
    }

    protected internal override void OnCreate()
    {
        base.OnCreate();

        Directional = true;
        DefaultDirection = Direction.LEFT;

        PaletteName = "bombBeenPalette";
        SpriteSheetName = "BombBeen";

        SetAnimationNames("BombDropped", "BombPlanted", "BombIdle", "BombAboutToExplode");

        SetupStateArray<BombBeenBombState>();
        RegisterState(BombBeenBombState.DROPPED, OnDropped, "BombDropped");
        RegisterState(BombBeenBombState.PLANTED, OnPlanted, "BombPlanted");
        RegisterState(BombBeenBombState.IDLE, OnIdle, "BombIdle");
        RegisterState(BombBeenBombState.ABOUT_TO_EXPLODE, OnAboutToExplode, "BombAboutToExplode");
    }

    private void OnDropped(EntityState state, long frameCounter)
    {
    }

    private void OnPlanted(EntityState state, long frameCounter)
    {
        tick++;

        if (frameCounter >= BombBeen.BOMB_PLANT_FRAMES)
            State = BombBeenBombState.IDLE;
    }

    private void OnIdle(EntityState state, long frameCounter)
    {
        tick++;
        if (tick + BombBeen.BOMB_ABOUT_TO_EXPLODE_FRAMES >= FramesToExplode)
            State = BombBeenBombState.ABOUT_TO_EXPLODE;
    }

    private void OnAboutToExplode(EntityState state, long frameCounter)
    {
        tick++;

        if (tick >= FramesToExplode)
            Break();
    }

    protected override void OnLanded()
    {
        base.OnLanded();

        Velocity = Vector.NULL_VECTOR;
        State = BombBeenBombState.PLANTED;
    }

    protected override Box GetHitbox()
    {
        return BombBeen.BOMB_HITBOX;
    }

    protected override Box GetCollisionBox()
    {
        return BombBeen.BOMB_COLLISION_BOX;
    }

    protected internal override void OnSpawn()
    {
        base.OnSpawn();

        Health = BombBeen.BOMB_HEALTH;

        tick = 0;

        State = BombBeenBombState.DROPPED;
    }

    protected override void OnBroke()
    {
        base.OnBroke();

        Engine.CreateExplosionEffect(Origin);

        var player = Engine.Player;
        if (player != null && IsTouching(player))
            Hurt(player, BombBeen.BOMB_EXPLOSION_DAMAGE);
    }
}