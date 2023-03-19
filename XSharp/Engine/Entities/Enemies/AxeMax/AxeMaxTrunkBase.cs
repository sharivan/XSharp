using XSharp.Math;
using XSharp.Math.Geometry;
using XSharp.Engine.Collision;
using System.Collections.Generic;
using XSharp.Engine.Graphics;

namespace XSharp.Engine.Entities.Enemies.AxeMax;

public class AxeMaxTrunkBase : Sprite
{
    #region StaticFields
    public static readonly Box HITBOX = ((0, 0), (-13, -11), (13, 6));
    #endregion

    #region Precache
    [Precache]
    internal static void Precache()
    {
        Engine.CallPrecacheAction(typeof(AxeMax));
    }
    #endregion

    private EntityReference<AxeMax> axeMax;
    private List<EntityReference<AxeMaxTrunk>> trunkPile;

    public bool IsReady => trunkPile.Count == TrunkCount && IsAllTrunksReady();

    public bool FirstRegenerating
    {
        get;
        private set;
    }

    public bool Regenerating
    {
        get;
        private set;
    }

    public AxeMax AxeMax
    {
        get => axeMax;
        internal set => axeMax = value;
    }

    public int TrunkCount => AxeMax.TrunkCount;

    public int ThrownTrunkCount
    {
        get;
        private set;
    }

    public IReadOnlyList<EntityReference<AxeMaxTrunk>> TrunkPile => trunkPile;

    public AxeMaxTrunkBase()
    {
    }

    protected internal override void OnCreate()
    {
        base.OnCreate();

        PaletteName = "axeMaxPalette";
        SpriteSheetName = "AxeMax";
        Directional = false;

        SetAnimationNames("TrunkBase");
        InitialAnimationName = "TrunkBase";

        trunkPile = new List<EntityReference<AxeMaxTrunk>>();
    }

    public override FixedSingle GetGravity()
    {
        return 0;
    }

    protected override Box GetHitbox()
    {
        return HITBOX;
    }

    private bool IsAllTrunksReady()
    {
        foreach (var trunkRef in trunkPile)
        {
            var trunk = (AxeMaxTrunk) trunkRef;
            if (trunk == null || !trunk.Landed || !trunk.Idle)
                return false;
        }

        return true;
    }

    internal Vector GetTrunkPositionFromIndex(int index)
    {
        var origin = IntegerOrigin;
        return (origin.X, origin.Y - 16 * (index + 1));
    }

    protected internal override void OnSpawn()
    {
        base.OnSpawn();

        KillOnOffscreen = false;
        CheckCollisionWithWorld = false;
        CollisionData = CollisionData.SOLID;

        FirstRegenerating = true;
        Regenerating = true;
        ThrownTrunkCount = 0;

        trunkPile.Clear();
        SpawnTrunk(0);
    }

    private void SpawnTrunk(int index)
    {
        AxeMaxTrunk trunk = Engine.Entities.Create<AxeMaxTrunk>();
        trunkPile.Add(trunk);

        trunk.TrunkBase = this;
        trunk.Origin = GetTrunkPositionFromIndex(index);
        trunk.TrunkIndex = index;
        trunk.Spawn();
    }

    protected override void Think()
    {
        base.Think();

        if (!Regenerating)
        {
            if (trunkPile.Count < TrunkCount)
            {
                if (ThrownTrunkCount == 0 && !AxeMax.Lumberjack.Throwing)
                {
                    Regenerating = true;

                    if (IsAllTrunksReady())
                        SpawnTrunk(trunkPile.Count);
                }
            }
        }
        else
        {
            if (trunkPile.Count == TrunkCount)
                Regenerating = false;
            else if (IsAllTrunksReady())
                SpawnTrunk(trunkPile.Count);
        }
    }

    protected override void OnDeath()
    {
        foreach (var trunkRef in trunkPile)
        {
            var trunk = (AxeMaxTrunk) trunkRef;
            if (trunk != null)
            {
                trunk.TrunkBase = null;
                trunk.Kill();
            }
        }

        trunkPile.Clear();

        base.OnDeath();
    }

    internal void ThrowTrunk()
    {
        if (trunkPile.Count > 0)
        {
            var trunk = (AxeMaxTrunk) trunkPile[0];
            if (trunk != null && trunk.Landed && trunk.Idle)
            {
                trunkPile.RemoveAt(0);

                ThrownTrunkCount++;
                trunk.Throw(AxeMax.Direction);

                for (int i = 0; i < trunkPile.Count; i++)
                {
                    trunk = (AxeMaxTrunk) trunkPile[i];
                    trunk.TrunkIndex = i;
                }
            }
        }
    }

    internal void NotifyTrunkReady(AxeMaxTrunk trunk)
    {
        if (trunkPile.Count < TrunkCount)
            SpawnTrunk(trunkPile.Count);
        else
            FirstRegenerating = false;
    }

    internal void NotifyTrunkDeath(AxeMaxTrunk trunk)
    {
        if (trunk.Thrown)
            ThrownTrunkCount--;
        else
        {
            trunkPile.Remove(trunk);

            for (int i = 0; i < trunkPile.Count; i++)
            {
                trunk = (AxeMaxTrunk) trunkPile[i];
                if (trunk != null)
                    trunk.TrunkIndex = i;
            }
        }
    }
}