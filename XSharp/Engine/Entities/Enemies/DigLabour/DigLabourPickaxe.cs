using SharpDX;

using XSharp.Engine.Graphics;
using XSharp.Math;
using XSharp.Math.Geometry;

namespace XSharp.Engine.Entities.Enemies.DigLabour;

public class DigLabourPickaxe : Enemy
{
    #region Precache
    [Precache]
    new internal static void Precache()
    {
        Engine.CallPrecacheAction<DigLabour>();
    }
    #endregion

    internal EntityReference<DigLabour> pitcher;

    public DigLabour Pitcher => pitcher;

    public DigLabourPickaxe()
    {
    }

    protected override void OnCreate()
    {
        base.OnCreate();

        DefaultDirection = Direction.LEFT;
        SpawnFacedToPlayer = false;

        PaletteName = "DigLabourPalette";
        SpriteSheetName = "DigLabour";

        SetAnimationNames("Pickaxe");
        InitialAnimationName = "Pickaxe";
    }

    protected override Box GetHitbox()
    {
        return DigLabour.PICKAXE_HITBOX;
    }

    protected override void OnSpawn()
    {
        base.OnSpawn();

        CheckCollisionWithWorld = false;
        ContactDamage = DigLabour.PICKAXE_DAMAGE;
        HitResponse = HitResponse.IGNORE;
        Invincible = true; 
    }

    protected override void OnContactDamage(Player player)
    {
        base.OnContactDamage(player);

        Pitcher?.NotifyPlayerDamagedByPickaxe();
    }
}