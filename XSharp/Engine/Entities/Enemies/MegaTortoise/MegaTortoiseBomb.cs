using XSharp.Engine.Graphics;
using XSharp.Math.Fixed;
using XSharp.Math.Fixed.Geometry;

namespace XSharp.Engine.Entities.Enemies.MegaTortoise;

public enum MegaTortoiseBombState
{
    LAUNCHING,
    PRE_FALLING,
    FALLING,
    EXPLODING
}

public class MegaTortoiseBomb : Enemy, IFSMEntity<MegaTortoiseBombState>
{
    #region Precache
    [Precache]
    new internal static void Precache()
    {
        Engine.CallPrecacheAction<MegaTortoise>();
    }
    #endregion

    internal EntityReference<MegaTortoise> launcher;

    public MegaTortoiseBombState State
    {
        get => GetState<MegaTortoiseBombState>();
        set => SetState(value);
    }

    public MegaTortoise Launcher => launcher;

    public MegaTortoiseBomb()
    {
    }

    protected override void OnCreate()
    {
        base.OnCreate();

        DefaultDirection = Direction.LEFT;
        SpawnFacedToPlayer = false;

        PaletteName = "MegaTortoisePalette";
        SpriteSheetName = "MegaTortoise";

        SetAnimationNames("BombLaunching", "BombPreFalling", "BombFalling");

        SetupStateArray<MegaTortoiseBombState>();
        RegisterState(MegaTortoiseBombState.LAUNCHING, OnLauching, "BombLaunching");
        RegisterState(MegaTortoiseBombState.PRE_FALLING, OnPreFalling, "BombPreFalling");
        RegisterState(MegaTortoiseBombState.FALLING, OnFalling, "BombFalling");
        RegisterState(MegaTortoiseBombState.EXPLODING, OnStartExploding, OnExploding, null);
    }

    private void OnLauching(EntityState state, long frameCounter)
    {
        if (frameCounter >= MegaTortoise.BOMB_FRAME_TO_OPEN_PARACHUTE)
            State = MegaTortoiseBombState.PRE_FALLING;
    }

    private void OnPreFalling(EntityState state, long frameCounter)
    {
        if (frameCounter >= MegaTortoise.BOMB_PRE_FALLING_FRAMES)
            State = MegaTortoiseBombState.FALLING;
    }

    private void OnFalling(EntityState state, long frameCounter)
    {
        Velocity = Velocity.YVector;

        if (Landed)
            State = MegaTortoiseBombState.EXPLODING;
    }

    private void OnStartExploding(EntityState state, EntityState lastState)
    {
        Engine.CreateExplosionEffect(Origin);
        ContactDamage = MegaTortoise.BOMB_EXPLOSION_DAMAGE;
    }

    private void OnExploding(EntityState state, long frameCounter)
    {
        Velocity = Vector.NULL_VECTOR;

        if (frameCounter >= MegaTortoise.BOMB_EXPLOSION_FRAMES)
            Kill();
    }

    public override FixedSingle GetGravity()
    {
        return State == MegaTortoiseBombState.LAUNCHING ? MegaTortoise.BOMB_LAUCHING_GRAVITY : 0;
    }

    protected override Box GetHitbox()
    {
        return State == MegaTortoiseBombState.EXPLODING ? MegaTortoise.BOMB_EXPLOSION_HITBOX : MegaTortoise.BOMB_HITBOX;
    }

    protected override void OnSpawn()
    {
        base.OnSpawn();

        Health = MegaTortoise.BOMB_HEALTH;
        ContactDamage = 0;

        State = MegaTortoiseBombState.LAUNCHING;
    }

    protected override void OnStartTouch(Entity entity)
    {
        base.OnStartTouch(entity);

        if (entity is Player)
            State = MegaTortoiseBombState.EXPLODING;
    }
}