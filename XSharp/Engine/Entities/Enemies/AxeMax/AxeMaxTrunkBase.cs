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

    public bool IsReady => trunkPile.Count == TrunkCount && IsAllTrunksLanded();

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

    private bool IsAllTrunksLanded()
    {
        foreach (var trunkRef in trunkPile)
        {
            var trunk = (AxeMaxTrunk) trunkRef;
            if (!trunk.Landed)
                return false;
        }

        return true;
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
        trunk.TrunkBase = this;
        trunk.Origin = (Origin.X, Origin.Y - 16 * (index + 1));
        trunk.Spawn();
    }

    protected override void Think()
    {
        base.Think();

        if (!Regenerating)
        {
            if (trunkPile.Count < TrunkCount && ThrownTrunkCount == 0 && IsAllTrunksLanded() && !AxeMax.Lumberjack.Throwing)
            {
                Regenerating = true;
                SpawnTrunk(trunkPile.Count);
            }
        }
        else if (trunkPile.Count == TrunkCount)
        {
            Regenerating = false;
        }
    }

    protected override void OnDeath()
    {
        foreach (var trunkRef in trunkPile)
        {
            var trunk = (AxeMaxTrunk) trunkRef;
            trunk.TrunkBase = null;
            trunk.Kill();
        }

        trunkPile.Clear();

        base.OnDeath();
    }

    internal void ThrowTrunk()
    {
        if (trunkPile.Count > 0)
        {
            var trunk = (AxeMaxTrunk) trunkPile[0];
            if (trunk.Landed)
            {
                trunkPile.RemoveAt(0);

                trunk.Throw(AxeMax.Direction);
                ThrownTrunkCount++;
            }
        }
    }

    internal void NotifyTrunkReady(AxeMaxTrunk trunk)
    {
        trunkPile.Add(trunk);
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
            trunkPile.Remove(trunk);
    }
}