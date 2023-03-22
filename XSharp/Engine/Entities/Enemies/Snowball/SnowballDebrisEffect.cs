using XSharp.Engine.Entities.Effects;
using XSharp.Engine.Graphics;

namespace XSharp.Engine.Entities.Enemies.Snowball;

public enum SnowballDebrisType
{
    SMALLEST = 0,
    SMALL = 1,
    MEDIUM = 2,
    BIG = 3
}

public class SnowballDebrisEffect : SpriteEffect
{
    #region StaticFields
    private static readonly string[] ANIMATION_NAMES = { "SmallestDebris", "SmallDebris", "MediumDebris", "BigDebris" };
    #endregion

    #region Precache
    [Precache]
    new internal static void Precache()
    {
        Engine.CallPrecacheAction<Snowball>();
    }
    #endregion

    private SnowballDebrisType debrisType = SnowballDebrisType.SMALL;
    internal int initialBlinkFrame = 3;

    public SnowballDebrisType DebrisType
    {
        get => debrisType;
        set
        {
            debrisType = value;

            if (ResourcesCreated)
                SetCurrentAnimationByName(ANIMATION_NAMES[(int) value]);
        }
    }

    public SnowballDebrisEffect()
    {
    }

    protected override void OnCreate()
    {
        base.OnCreate();

        Directional = true;
        DefaultDirection = Direction.LEFT;

        PaletteName = "snowballPalette";
        SpriteSheetName = "Snowball";

        SetAnimationNames(ANIMATION_NAMES);
    }

    protected override void OnSpawn()
    {
        base.OnSpawn();

        HasGravity = true;
        KillOnOffscreen = true;
        Blinking = false;

        SetCurrentAnimationByName(ANIMATION_NAMES[(int) debrisType]);        
    }

    protected override void OnThink()
    {
        base.OnThink();

        if (FrameCounter == initialBlinkFrame)
            Blinking = true;
    }
}