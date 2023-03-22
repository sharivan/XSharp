using System.Reflection;

using SharpDX;

using XSharp.Engine.Graphics;
using XSharp.Math;
using XSharp.Math.Geometry;

namespace XSharp.Engine.Entities.Enemies.AxeMax;

public class AxeMax : Sprite
{
    #region StaticFields
    public static readonly Color[] AXE_MAX_PALETTE = new Color[]
    {
        Color.Transparent, // 0
        Color.FromBgra(0xFF386080), // 1
        Color.FromBgra(0xFF6090B0), // 2
        Color.FromBgra(0xFFA8C8E8), // 3
        Color.FromBgra(0xFFB85820), // 4
        Color.FromBgra(0xFFE8A040), // 5           
        Color.FromBgra(0xFFF8D888), // 6
        Color.FromBgra(0xFF287828), // 7
        Color.FromBgra(0xFF78B058), // 8
        Color.FromBgra(0xFFC8F0A0), // 9
        Color.FromBgra(0xFF705870), // A
        Color.FromBgra(0xFFA090A0), // B
        Color.FromBgra(0xFFE0D0E0), // C
        Color.FromBgra(0xFF783830), // D
        Color.FromBgra(0xFFF87858), // E
        Color.FromBgra(0xFF302820) // F
    };

    public const int TRUNK_COUNT = 2;
    #endregion

    #region Precache
    [Precache]
    internal static void Precache()
    {
        var palette = Engine.PrecachePalette("axeMaxPalette", AXE_MAX_PALETTE);
        var spriteSheet = Engine.CreateSpriteSheet("AxeMax", true, true);

        using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("XSharp.resources.sprites.Enemies.X.AxeMax.png"))
        {
            var texture = Engine.CreateImageTextureFromStream(stream);
            spriteSheet.CurrentTexture = texture;
        }

        spriteSheet.CurrentPalette = palette;

        var sequence = spriteSheet.AddFrameSquence("Idle");
        sequence.OriginOffset = -AxeMaxLumberjack.HITBOX.Origin - AxeMaxLumberjack.HITBOX.Mins;
        sequence.Hitbox = AxeMaxLumberjack.HITBOX;
        sequence.AddFrame(-4, 4, 200, 4, 39, 40, 1, true);

        sequence = spriteSheet.AddFrameSquence("Laughing");
        sequence.OriginOffset = -AxeMaxLumberjack.HITBOX.Origin - AxeMaxLumberjack.HITBOX.Mins;
        sequence.Hitbox = AxeMaxLumberjack.HITBOX;
        sequence.AddFrame(-4, 4, 200, 4, 39, 40, 6, true);
        sequence.AddFrame(-4, 4, 250, 4, 39, 40, 6);
        sequence.AddFrame(-4, 4, 300, 4, 39, 40, 6);
        sequence.AddFrame(-4, 4, 250, 4, 39, 40, 6); // total of 24 frames

        sequence = spriteSheet.AddFrameSquence("Throwing");
        sequence.OriginOffset = -AxeMaxLumberjack.HITBOX.Origin - AxeMaxLumberjack.HITBOX.Mins;
        sequence.Hitbox = AxeMaxLumberjack.HITBOX;
        sequence.AddFrame(-7, 4, 298, 54, 43, 40, 3);
        sequence.AddFrame(-7, 4, 254, 54, 32, 40, 3);
        sequence.AddFrame(-7, 4, 298, 54, 43, 40, 3);
        sequence.AddFrame(-6, 4, 5, 54, 29, 40, 3);
        sequence.AddFrame(-2, 4, 58, 54, 23, 40, 3);
        sequence.AddFrame(3, 4, 105, 54, 29, 40, 3);
        sequence.AddFrame(5, 4, 153, 54, 33, 40, 3); // <= start throwing trunks here
        sequence.AddFrame(-9, 4, 210, 54, 20, 40, 3);
        sequence.AddFrame(-10, 4, 355, 54, 23, 40, 3);
        sequence.AddFrame(-7, 4, 298, 54, 43, 40, 3);
        sequence.AddFrame(-6, 4, 5, 54, 29, 40, 3);
        sequence.AddFrame(-2, 4, 58, 54, 23, 40, 3);
        sequence.AddFrame(-6, 4, 5, 54, 29, 40, 3);
        sequence.AddFrame(-2, 4, 58, 54, 23, 40, 3);
        sequence.AddFrame(-6, 4, 5, 54, 29, 40, 3);
        sequence.AddFrame(-7, 4, 298, 54, 43, 40, 2);
        sequence.AddFrame(-4, 4, 200, 4, 39, 40, 1); // total of 48 frames

        sequence = spriteSheet.AddFrameSquence("TrunkBase");
        sequence.OriginOffset = -AxeMaxTrunkBase.HITBOX.Origin - AxeMaxTrunkBase.HITBOX.Mins;
        sequence.Hitbox = AxeMaxTrunkBase.HITBOX;
        sequence.AddFrame(1, -3, 106, 16, 28, 16, 1, true);

        sequence = spriteSheet.AddFrameSquence("TrunkIdle");
        sequence.OriginOffset = -AxeMaxTrunk.IDLE_HITBOX.Origin - AxeMaxTrunk.IDLE_HITBOX.Mins;
        sequence.Hitbox = AxeMaxTrunk.IDLE_HITBOX;
        sequence.AddFrame(-1, -3, 58, 8, 24, 16, 1, true);

        sequence = spriteSheet.AddFrameSquence("TrunkThrown");
        sequence.OriginOffset = -AxeMaxTrunk.IDLE_HITBOX.Origin - AxeMaxTrunk.IDLE_HITBOX.Mins;
        sequence.Hitbox = AxeMaxTrunk.IDLE_HITBOX;
        sequence.AddFrame(3, -2, 154, 15, 32, 18, 1, true);

        spriteSheet.ReleaseCurrentTexture();
    }
    #endregion

    private EntityReference<AxeMaxTrunkBase> trunkBase;
    private EntityReference<AxeMaxLumberjack> lumberjack;

    public AxeMaxTrunkBase TrunkBase => trunkBase;

    public AxeMaxLumberjack Lumberjack => lumberjack;

    public int TrunkCount { get; set; } = TRUNK_COUNT;

    public AxeMax()
    {
    }

    protected override void OnCreate()
    {
        base.OnCreate();

        KillOnOffscreen = true;

        trunkBase = Engine.Entities.Create<AxeMaxTrunkBase>();
        TrunkBase.AxeMax = this;

        lumberjack = Engine.Entities.Create<AxeMaxLumberjack>();
        Lumberjack.AxeMax = this;

        Directional = true;
        DefaultDirection = Direction.LEFT;
    }

    public override FixedSingle GetGravity()
    {
        return 0;
    }

    protected override Box GetHitbox()
    {
        return AxeMaxTrunkBase.HITBOX;
    }

    protected override void OnSpawn()
    {
        base.OnSpawn();

        Direction = DefaultDirection;

        TrunkBase.Respawnable = true;
        TrunkBase.Origin = Origin;
        TrunkBase.Parent = this;
        TrunkBase.Spawn();

        Lumberjack.Respawnable = true;
        Lumberjack.Origin = (Direction == Direction.LEFT ? Origin.X + 32 : Origin.X - 32, Origin.Y - 14);
        Lumberjack.Parent = this;
        Lumberjack.Spawn();
    }

    protected override void OnDeath()
    {
        TrunkBase.Respawnable = Respawnable;
        TrunkBase.Kill();

        Lumberjack.Respawnable = Respawnable;
        Lumberjack.Kill();

        base.OnDeath();
    }

    public void MakeLumberjackLaugh()
    {
        Lumberjack?.MakeLaughing();
    }
}