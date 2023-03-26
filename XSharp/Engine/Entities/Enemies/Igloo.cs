using XSharp.Engine.Graphics;
using XSharp.Math;
using XSharp.Math.Geometry;

namespace XSharp.Engine.Entities.Enemies;

public class Igloo : Enemy
{
    #region StaticFields
    public static readonly FixedSingle HP = 32;
    public static readonly Box HITBOX = ((0, 0), (-48, -24), (48, 24));

    public static readonly FixedSingle TOMBOT_SPAWN_OFFSET_Y = 38;
    public const int TOMBOT_FIRST_SPAWN_INTERVAL = 166;
    public const int TOMBOT_SPAWN_INTERVAL = 96;
    #endregion

    #region Precache
    [Precache]
    new internal static void Precache()
    {
        Engine.CallPrecacheAction<Tombot.Tombot>();
    }
    #endregion

    private bool firstSpawn;

    public Igloo()
    {
    }

    protected override void OnCreate()
    {
        base.OnCreate();
    }

    public override FixedSingle GetGravity()
    {
        return 0;
    }

    protected override Box GetHitbox()
    {
        return HITBOX;
    }

    protected override void OnSpawn()
    {
        base.OnSpawn();

        HitResponse = HitResponse.REFLECT; // TODO : This enemy should only reflect shots but Fire Wave (or another desired weapon).
        ContactDamage = 0;
        Health = HP;
        SpawnFacedToPlayer = false;

        firstSpawn = true;
    }

    protected override void OnThink()
    {
        base.OnThink();

        if (firstSpawn)
        {
            if (FrameCounter == TOMBOT_FIRST_SPAWN_INTERVAL)
            {
                firstSpawn = false;
                SpawnTombot();
            }
        }
        else if ((FrameCounter - TOMBOT_FIRST_SPAWN_INTERVAL) % TOMBOT_SPAWN_INTERVAL == 0)
        {
            SpawnTombot();
        }
    }

    public EntityReference<Tombot.Tombot> SpawnTombot()
    {
        var player = Engine.Player;

        Tombot.Tombot tombot = Engine.Entities.Create<Tombot.Tombot>(new
        {
            Origin = Origin + TOMBOT_SPAWN_OFFSET_Y * Vector.DOWN_VECTOR,
            Direction = player != null ? GetHorizontalDirection(player) : Direction.RIGHT
        });

        tombot.Spawn();
        return tombot;
    }

    internal void NotifyTombotDeath(EntityReference<Tombot.Tombot> tombot)
    {
    }

    protected override void OnBroke()
    {
        base.OnBroke();

        // TODO : Implement the break effect where debris of the igloo are spawned.
    }
}