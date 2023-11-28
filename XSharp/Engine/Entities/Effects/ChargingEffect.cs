using XSharp.Graphics;
using XSharp.Engine.Graphics;

namespace XSharp.Engine.Entities.Effects;

public class ChargingEffect : SpriteEffect
{
    #region StaticFields
    public static readonly Color[] CHARGE_EFFECT_PALETTE =
    [
        Color.Transparent, // 0
        new Color(136, 248, 248, 255), // 1
        new Color(248, 224, 112, 255), // 2
        new Color(248, 248, 248, 255), // 3
        new Color(240, 176, 56, 255), // 4
        new Color(240, 144, 96, 255) // 5
    ];

    public static readonly Color[] CHARGE_LEVEL_1_PALETTE =
        [
        Color.Transparent, // 0
        new Color(248, 248, 248, 255), // 1
        new Color(232, 224, 64, 255), // 2
        new Color(240, 104, 192, 255), // 3
        new Color(160, 240, 240, 255), // 4
        new Color(80, 216, 240, 255), // 5
        new Color(24, 128, 224, 255), // 6
        new Color(0, 184, 248, 255), // 7
        new Color(0, 144, 240, 255), // 8
        new Color(32, 104, 240, 255), // 9
        new Color(248, 176, 128, 255), // 10
        new Color(184, 96, 72, 255), // 11
        new Color(128, 64, 32, 255), // 12
        new Color(248, 248, 248, 255), // 13
        new Color(176, 176, 176, 255), // 14
        new Color(24, 80, 224, 255) // 15
        ];

    public static readonly Color[] CHARGE_LEVEL_2_PALETTE =
    [
        Color.Transparent, // 0
        new Color(248, 248, 248, 255), // 1
        new Color(232, 224, 64, 255), // 2
        new Color(240, 104, 192, 255), // 3
        new Color(224, 224, 248, 255), // 4
        new Color(200, 176, 248, 255), // 5
        new Color(152, 136, 240, 255), // 6
        new Color(176, 168, 248, 255), // 7
        new Color(176, 136, 248, 255), // 8
        new Color(136, 112, 232, 255), // 9
        new Color(248, 176, 128, 255), // 10
        new Color(184, 96, 72, 255), // 11
        new Color(128, 64, 32, 255), // 12
        new Color(248, 248, 248, 255), // 13
        new Color(176, 176, 176, 255), // 14
        new Color(144, 0, 216, 255) // 15
    ];
    #endregion

    #region Precache
    [Precache]
    new internal static void Precache()
    {
        Engine.PrecachePalette("chargeLevel1Palette", CHARGE_LEVEL_1_PALETTE);
        Engine.PrecachePalette("chargeLevel2Palette", CHARGE_LEVEL_2_PALETTE);
        Engine.PrecachePalette("chargingEffectPalette", CHARGE_EFFECT_PALETTE);

        var xChargingEffectsSpriteSheet = Engine.CreateSpriteSheet("X Charging Effects", true, false);

        var sequence = xChargingEffectsSpriteSheet.AddFrameSquence("ChargingLevel1");
        Engine.AddChargingEffectFrames(sequence, 1);

        sequence = xChargingEffectsSpriteSheet.AddFrameSquence("ChargingLevel2");
        Engine.AddChargingEffectFrames(sequence, 2);
    }
    #endregion

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
        PaletteName = "chargingEffectPalette";

        SetAnimationNames("ChargingLevel1", "ChargingLevel2");
    }

    protected override void OnSpawn()
    {
        base.OnSpawn();

        Origin = Charger.Hitbox.Center;
        Parent = Charger;
        Direction = Charger.Direction;

        level = 1;
    }

    protected override void OnThink()
    {
        if (!soundPlayed)
        {
            Engine.PlaySound(2, "X Charge", 3.350, 1.585);
            soundPlayed = true;
        }

        base.OnThink();
    }

    protected override void OnDeath()
    {
        Engine.StopSound(2, "X Charge");
        base.OnDeath();
    }
}