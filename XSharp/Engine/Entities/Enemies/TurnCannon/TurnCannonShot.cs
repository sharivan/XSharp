using XSharp.Engine.Graphics;
using XSharp.Math;
using XSharp.Math.Geometry;

namespace XSharp.Engine.Entities.Enemies.TurnCannon;

public class TurnCannonShot : Enemy
{
    #region Precache
    [Precache]
    new internal static void Precache()
    {
        Engine.CallPrecacheAction(typeof(TurnCannon));
    }
    #endregion

    public TurnCannonShot()
    {
    }

    protected override void OnCreate()
    {
        base.OnCreate();

        SpawnFacedToPlayer = false;

        PaletteName = "TurnCannonPalette";
        SpriteSheetName = "TurnCannon";

        SetAnimationNames("Shot");
        InitialAnimationName = "Shot";
    }

    public override FixedSingle GetGravity()
    {
        return 0;
    }

    protected override Box GetHitbox()
    {
        return TurnCannon.SHOT_HITBOX;
    }

    protected override void OnSpawn()
    {
        base.OnSpawn();

        CheckCollisionWithWorld = false;
        ContactDamage = TurnCannon.SHOT_DAMAGE;
        HitResponse = HitResponse.IGNORE;
        Invincible = true;
    }
}