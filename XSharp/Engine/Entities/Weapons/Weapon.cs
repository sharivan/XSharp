using System.Reflection;

using XSharp.Engine.Entities.Enemies;
using XSharp.Engine.Graphics;
using XSharp.Math;
using XSharp.Math.Geometry;

using static XSharp.Engine.Consts;

namespace XSharp.Engine.Entities.Weapons;

public abstract class Weapon : Sprite
{
    #region Precache
    [Precache]
    internal static void Precache()
    {
        Engine.PrecacheSound("Enemy Helmet Hit", @"resources\sounds\mmx\29 - MMX - Enemy Helmet Hit.wav");

        var xWeaponsSpriteSheet = Engine.CreateSpriteSheet("X Weapons", true, true);

        using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("XSharp.resources.sprites.X.Weapons.png"))
        {
            var texture = Engine.CreateImageTextureFromStream(stream);
            xWeaponsSpriteSheet.CurrentTexture = texture;
        }

        var sequence = xWeaponsSpriteSheet.AddFrameSquence("LemonShot", 0);
        sequence.OriginOffset = -LEMON_HITBOX.Origin - LEMON_HITBOX.Mins;
        sequence.Hitbox = LEMON_HITBOX;
        sequence.AddFrame(0, -1, 123, 253, 8, 6);

        sequence = xWeaponsSpriteSheet.AddFrameSquence("LemonShotExplode");
        sequence.OriginOffset = -LEMON_HITBOX.Origin - LEMON_HITBOX.Mins;
        sequence.Hitbox = LEMON_HITBOX;
        sequence.AddFrame(2, 1, 137, 250, 12, 12, 4);
        sequence.AddFrame(2, 2, 154, 249, 13, 13, 2);
        sequence.AddFrame(3, 3, 172, 248, 15, 15);

        sequence = xWeaponsSpriteSheet.AddFrameSquence("SemiChargedShotFiring");
        sequence.OriginOffset = -SEMI_CHARGED_HITBOX1.Origin - SEMI_CHARGED_HITBOX1.Mins;
        sequence.Hitbox = SEMI_CHARGED_HITBOX1;
        sequence.AddFrame(-5, -2, 128, 563, 14, 14);
        sequence.OriginOffset = -SEMI_CHARGED_HITBOX2.Origin - SEMI_CHARGED_HITBOX2.Mins;
        sequence.Hitbox = SEMI_CHARGED_HITBOX2;
        sequence.AddFrame(-9, -6, 128, 563, 14, 14);
        sequence.AddFrame(-9, -1, 147, 558, 24, 24);
        sequence.OriginOffset = -SEMI_CHARGED_HITBOX3.Origin - SEMI_CHARGED_HITBOX3.Mins;
        sequence.Hitbox = SEMI_CHARGED_HITBOX3;
        sequence.AddFrame(-11, 3, 147, 558, 24, 24);
        sequence.AddFrame(-11, -3, 176, 564, 28, 12);

        sequence = xWeaponsSpriteSheet.AddFrameSquence("SemiChargedShot");
        sequence.OriginOffset = -SEMI_CHARGED_HITBOX3.Origin - SEMI_CHARGED_HITBOX3.Mins;
        sequence.Hitbox = SEMI_CHARGED_HITBOX3;
        sequence.AddFrame(3, -3, 176, 564, 28, 12);
        sequence.AddFrame(3, -5, 210, 566, 32, 8, 3);
        sequence.AddFrame(9, -5, 210, 566, 32, 8);
        sequence.AddFrame(7, -1, 379, 562, 38, 16);
        sequence.AddFrame(9, -3, 333, 564, 38, 12, 1, true); // loop point
        sequence.AddFrame(8, 1, 292, 559, 36, 22, 2);
        sequence.AddFrame(9, -3, 333, 564, 38, 12);
        sequence.AddFrame(7, -1, 379, 562, 38, 16, 2);

        sequence = xWeaponsSpriteSheet.AddFrameSquence("SemiChargedShotHit");
        sequence.OriginOffset = -SEMI_CHARGED_HITBOX2.Origin - SEMI_CHARGED_HITBOX2.Mins;
        sequence.Hitbox = SEMI_CHARGED_HITBOX2;
        sequence.AddFrame(-9, -6, 424, 563, 14, 14, 2);
        sequence.AddFrame(-9, -1, 443, 558, 24, 24, 4);
        sequence.AddFrame(-9, -6, 424, 563, 14, 14, 4);

        sequence = xWeaponsSpriteSheet.AddFrameSquence("SemiChargedShotExplode");
        sequence.OriginOffset = -SEMI_CHARGED_HITBOX1.Origin - SEMI_CHARGED_HITBOX1.Mins;
        sequence.AddFrame(487, 273, 16, 16);
        sequence.AddFrame(507, 269, 24, 24);
        sequence.AddFrame(535, 273, 16, 16);
        sequence.AddFrame(555, 270, 22, 22);
        sequence.AddFrame(581, 269, 24, 24);
        sequence.AddFrame(609, 269, 24, 24);

        sequence = xWeaponsSpriteSheet.AddFrameSquence("ChargedShotFiring");
        sequence.OriginOffset = -CHARGED_HITBOX1.Origin - CHARGED_HITBOX1.Mins;
        sequence.Hitbox = CHARGED_HITBOX1;
        sequence.AddFrame(-3, 1, 144, 440, 14, 20);
        sequence.AddFrame(-2, -1, 170, 321, 23, 16, 3);
        sequence.OriginOffset = -CHARGED_HITBOX2.Origin - CHARGED_HITBOX2.Mins;
        sequence.Hitbox = CHARGED_HITBOX2;
        sequence.AddFrame(-25, -10, 170, 321, 23, 16, 3);

        sequence = xWeaponsSpriteSheet.AddFrameSquence("ChargedShot", 0);
        sequence.OriginOffset = -CHARGED_HITBOX2.Origin - CHARGED_HITBOX2.Mins;
        sequence.Hitbox = CHARGED_HITBOX2;
        sequence.AddFrame(7, -2, 164, 433, 47, 32, 2, true);
        sequence.AddFrame(2, -2, 216, 433, 40, 32, 2);
        sequence.AddFrame(9, -2, 261, 432, 46, 32, 2);

        sequence = xWeaponsSpriteSheet.AddFrameSquence("ChargedShotHit");
        sequence.OriginOffset = -CHARGED_HITBOX2.Origin - CHARGED_HITBOX2.Mins;
        sequence.Hitbox = CHARGED_HITBOX2;
        sequence.AddFrame(-26, -8, 315, 438, 14, 20, 2);
        sequence.AddFrame(-25, -4, 336, 434, 24, 28, 2);
        sequence.AddFrame(-26, -8, 315, 438, 14, 20, 4);

        sequence = xWeaponsSpriteSheet.AddFrameSquence("ChargedShotExplode");
        sequence.OriginOffset = -CHARGED_HITBOX2.Origin - CHARGED_HITBOX2.Mins;
        sequence.AddFrame(368, 434, 28, 28);
        sequence.AddFrame(400, 435, 26, 26);
        sequence.AddFrame(430, 434, 28, 28);
        sequence.AddFrame(462, 433, 30, 30);
        sequence.AddFrame(496, 432, 32, 32);
        sequence.AddFrame(532, 432, 32, 32);

        var smallHealthRecoverDroppingCollisionBox = new Box(Vector.NULL_VECTOR, (-4, -8), (4, 0));

        sequence = xWeaponsSpriteSheet.AddFrameSquence("SmallHealthRecoverDropping");
        sequence.OriginOffset = -smallHealthRecoverDroppingCollisionBox.Mins;
        sequence.Hitbox = smallHealthRecoverDroppingCollisionBox;
        sequence.AddFrame(0, 0, 6, 138, 8, 8);
        sequence.AddFrame(0, 0, 24, 114, 8, 8);
        sequence.AddFrame(0, 0, 6, 138, 8, 8, 1, true);

        var smallHealthRecoverIdleCollisionBox = new Box(Vector.NULL_VECTOR, (-5, -8), (5, 0));

        sequence = xWeaponsSpriteSheet.AddFrameSquence("SmallHealthRecoverIdle");
        sequence.OriginOffset = -smallHealthRecoverIdleCollisionBox.Mins;
        sequence.Hitbox = smallHealthRecoverIdleCollisionBox;
        sequence.AddFrame(0, 0, 22, 138, 10, 8, 1, true);
        sequence.AddFrame(0, 0, 40, 138, 10, 8, 2);
        sequence.AddFrame(0, 0, 58, 138, 10, 8, 2);
        sequence.AddFrame(0, 0, 40, 138, 10, 8, 2);
        sequence.AddFrame(0, 0, 22, 138, 10, 8, 1);

        var bigHealthRecoverDroppingCollisionBox = new Box(Vector.NULL_VECTOR, (-7, -12), (7, 0));

        sequence = xWeaponsSpriteSheet.AddFrameSquence("BigHealthRecoverDropping");
        sequence.OriginOffset = -bigHealthRecoverDroppingCollisionBox.Mins;
        sequence.Hitbox = bigHealthRecoverDroppingCollisionBox;
        sequence.AddFrame(0, 0, 3, 150, 14, 12);
        sequence.AddFrame(0, 0, 24, 114, 14, 12);
        sequence.AddFrame(0, 0, 3, 150, 14, 12, 1, true);

        var bigHealthRecoverIdleCollisionBox = new Box(Vector.NULL_VECTOR, (-8, -12), (8, 0));

        sequence = xWeaponsSpriteSheet.AddFrameSquence("BigHealthRecoverIdle");
        sequence.OriginOffset = -bigHealthRecoverIdleCollisionBox.Mins;
        sequence.Hitbox = bigHealthRecoverIdleCollisionBox;
        sequence.AddFrame(0, 0, 19, 150, 16, 12, 1, true);
        sequence.AddFrame(0, 0, 37, 150, 16, 12, 2);
        sequence.AddFrame(0, 0, 55, 150, 16, 12, 2);
        sequence.AddFrame(0, 0, 37, 150, 16, 12, 2);
        sequence.AddFrame(0, 0, 19, 150, 16, 12, 1);

        var smallAmmoRecoverCollisionBox = new Box(Vector.NULL_VECTOR, (-4, -8), (4, 0));

        sequence = xWeaponsSpriteSheet.AddFrameSquence("SmallAmmoRecover");
        sequence.OriginOffset = -smallAmmoRecoverCollisionBox.Mins;
        sequence.Hitbox = smallAmmoRecoverCollisionBox;
        sequence.AddFrame(0, 0, 84, 138, 8, 8, 2, true);
        sequence.AddFrame(0, 0, 100, 138, 8, 8, 2);
        sequence.AddFrame(0, 0, 116, 138, 8, 8, 2);
        sequence.AddFrame(0, 0, 100, 138, 8, 8, 2);

        var bigAmmoRecoverCollisionBox = new Box(Vector.NULL_VECTOR, (-7, -14), (7, 0));

        sequence = xWeaponsSpriteSheet.AddFrameSquence("BigAmmoRecover");
        sequence.OriginOffset = -bigAmmoRecoverCollisionBox.Mins;
        sequence.Hitbox = bigAmmoRecoverCollisionBox;
        sequence.AddFrame(0, 0, 81, 148, 14, 14, 2, true);
        sequence.AddFrame(0, 0, 97, 148, 14, 14, 2);
        sequence.AddFrame(0, 0, 113, 148, 14, 14, 2);
        sequence.AddFrame(0, 0, 97, 148, 14, 14, 2);

        var lifeUpCollisionBox = new Box(Vector.NULL_VECTOR, (-8, -16), (8, 0));

        sequence = xWeaponsSpriteSheet.AddFrameSquence("LifeUp");
        sequence.OriginOffset = -lifeUpCollisionBox.Mins;
        sequence.Hitbox = lifeUpCollisionBox;
        sequence.AddFrame(0, 0, 137, 146, 16, 16, 4, true);
        sequence.AddFrame(0, 0, 157, 146, 16, 16, 4);

        var heartTankCollisionBox = new Box(Vector.NULL_VECTOR, (-8, -17), (8, 0));

        sequence = xWeaponsSpriteSheet.AddFrameSquence("HeartTank");
        sequence.OriginOffset = -heartTankCollisionBox.Mins;
        sequence.Hitbox = heartTankCollisionBox;
        sequence.AddFrame(-1, 0, 183, 147, 14, 15, 11, true);
        sequence.AddFrame(-2, -1, 199, 147, 12, 15, 11);
        sequence.AddFrame(-3, -2, 213, 147, 10, 15, 11);
        sequence.AddFrame(-2, -1, 225, 147, 12, 15, 11);

        var subTankCollisionBox = new Box(Vector.NULL_VECTOR, (-8, -19), (8, 0));

        sequence = xWeaponsSpriteSheet.AddFrameSquence("SubTank");
        sequence.OriginOffset = -subTankCollisionBox.Mins;
        sequence.Hitbox = subTankCollisionBox;
        sequence.AddFrame(2, 0, 247, 143, 20, 19, 4, true);
        sequence.AddFrame(2, 0, 269, 143, 20, 19, 4);

        xWeaponsSpriteSheet.ReleaseCurrentTexture();
    }
    #endregion

    private EntityReference<Sprite> shooter;

    public Sprite Shooter
    {
        get => shooter;
        protected set => shooter = Engine.Entities.GetReferenceTo(value);
    }

    public FixedSingle BaseDamage => GetBaseDamage();

    public FixedSingle Damage
    {
        get;
        set;
    }

    protected Weapon()
    {
    }

    protected override void OnCreate()
    {
        base.OnCreate();

        Directional = true;
        CanGoOutOfMapBounds = true;
    }

    protected virtual FixedSingle GetBaseDamage()
    {
        return 1;
    }

    protected override void OnSpawn()
    {
        base.OnSpawn();

        KillOnOffscreen = true;
        Damage = GetBaseDamage();
    }

    protected override void OnHurt(Sprite victim, FixedSingle damage)
    {
        base.OnHurt(victim, damage);

        if (victim is Enemy enemy)
            OnHit(enemy, damage);
    }

    internal void NotifyHit(Enemy enemy, FixedSingle damage)
    {
        OnHit(enemy, damage);
    }

    protected virtual void OnHit(Enemy enemy, FixedSingle damage)
    {
        Engine.PlaySound(1, "Small Hit");
    }

    public void Hit(Enemy enemy)
    {
        if (Damage > 0)
            Hurt(enemy, Damage);
    }

    protected override void OnStartTouch(Entity entity)
    {
        base.OnStartTouch(entity);

        if (Alive && !MarkedToRemove && entity is Enemy enemy)
            Hit(enemy);
    }

    public virtual void Reflect()
    {
    }
}