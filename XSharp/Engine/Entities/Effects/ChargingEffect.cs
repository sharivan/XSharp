using XSharp.Engine.Graphics;

using static XSharp.Engine.Consts;

namespace XSharp.Engine.Entities.Effects;

public class ChargingEffect : SpriteEffect
{
    [Precache]
    new internal static void Precache()
    {
        Engine.CreatePalette("chargeLevel1Palette", CHARGE_LEVEL_1_PALETTE);
        Engine.CreatePalette("chargeLevel2Palette", CHARGE_LEVEL_2_PALETTE);
        Engine.CreatePalette("chargingEffectPalette", CHARGE_EFFECT_PALETTE);

        var xChargingEffectsSpriteSheet = Engine.CreateSpriteSheet("X Charging Effects", true, false);

        var sequence = xChargingEffectsSpriteSheet.AddFrameSquence("ChargingLevel1");
        Engine.AddChargingEffectFrames(sequence, 1);

        sequence = xChargingEffectsSpriteSheet.AddFrameSquence("ChargingLevel2");
        Engine.AddChargingEffectFrames(sequence, 2);
    }

    private int level;

    private bool soundPlayed;

    private EntityReference<Player> charger;

    public Player Charger
    {
        get => charger;
        set
        {
            charger = value;
            if (value != null)
            {
                Origin = value.Hitbox.Center;
                Direction = value.Direction;
            }
        }
    }

    public int Level
    {
        get => level;

        set
        {
            if (level is < 1 or > 2)
                return;

            level = value;
            CurrentAnimationIndex = level - 1;
        }
    }

    public ChargingEffect()
    {
        SpriteSheetName = "X Charging Effects";
        Directional = true;
        PaletteName = "chargingEffectPalette";

        SetAnimationNames("ChargingLevel1", "ChargingLevel2");
    }

    protected internal override void OnSpawn()
    {
        base.OnSpawn();

        level = 1;
    }

    protected override void Think()
    {
        if (!soundPlayed)
        {
            Engine.PlaySound(2, "X Charge", 3.350, 1.585);
            soundPlayed = true;
        }

        Origin = Charger.Hitbox.Center;
        Direction = Charger.Direction;

        base.Think();
    }

    protected override void OnDeath()
    {
        Engine.StopSound(2, "X Charge");
        base.OnDeath();
    }
}