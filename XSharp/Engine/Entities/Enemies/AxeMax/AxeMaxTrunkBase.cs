using System.Collections.Generic;

using XSharp.Engine.Collision;
using XSharp.Engine.Graphics;
using XSharp.Math.Fixed;
using XSharp.Math.Fixed.Geometry;
using XSharp.Util;

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
        Engine.CallPrecacheAction<AxeMax>();
    }
    #endregion

    private EntityReference<AxeMax> axeMax;
    private List<EntityReference<AxeMaxTrunk>> trunkPile;
    internal BitSet readyTrunks;

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
        internal set;
    }

    public IReadOnlyList<EntityReference<AxeMaxTrunk>> TrunkPile => trunkPile;

    public AxeMaxTrunkBase()
    {
    }

    protected override void OnCreate()
    {
        base.OnCreate();

        PaletteName = "axeMaxPalette";
        SpriteSheetName = "AxeMax";

        SetAnimationNames("TrunkBase");
        InitialAnimationName = "TrunkBase";

        trunkPile = [];
        readyTrunks = [];
    }

    public override FixedSingle GetGravity()
    {
        return 0;
    }

    protected override Box GetHitbox()
    {
        return HITBOX;
    }

    internal bool IsAllTrunksReady()
    {
        return trunkPile.Count == 0 || readyTrunks.TestRange(0, trunkPile.Count);
    }

    internal Vector GetTrunkPositionFromIndex(int index)
    {
        var origin = PixelOrigin;
        return (origin.X, origin.Y - 16 * (index + 1));
    }

    protected override void OnSpawn()
    {
        base.OnSpawn();

        KillOnOffscreen = false;
        CheckCollisionWithWorld = false;
        CollisionData = CollisionData.SOLID;

        FirstRegenerating = true;
        Regenerating = true;
        ThrownTrunkCount = 0;

        trunkPile.Clear();
        readyTrunks.Clear();

        for (int i = 0; i < TrunkCount; i++)
            SpawnTrunk(i);
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

    protected override void OnThink()
    {
        base.OnThink();

        if (!Regenerating)
        {
            if (trunkPile.Count < TrunkCount && ThrownTrunkCount == 0 && !AxeMax.Lumberjack.Throwing && IsAllTrunksReady())
            {
                Regenerating = true;
                SpawnTrunk(trunkPile.Count);
            }
        }
        else if (IsAllTrunksReady())
        {
            if (trunkPile.Count == TrunkCount)
                Regenerating = false;
            else
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
        readyTrunks.Clear();

        base.OnDeath();
    }

    internal void ThrowTrunk()
    {
        if (trunkPile.Count > 0)
        {
            var trunk = (AxeMaxTrunk) trunkPile[0];
            if (trunk != null && trunk.Ready)
            {
                trunkPile.RemoveAt(0);
                readyTrunks.Clear();

                trunk.Throw(AxeMax.Direction);

                for (int i = 0; i < trunkPile.Count; i++)
                {
                    trunk = (AxeMaxTrunk) trunkPile[i];

                    if (trunk != null)
                    {
                        trunk.TrunkIndex = i;

                        if (trunk.Ready)
                            readyTrunks.Set(i);
                        else
                            readyTrunks.Reset(i);
                    }
                    else
                        readyTrunks.Reset(i);
                }
            }
        }
    }

    internal void NotifyTrunkReady(AxeMaxTrunk trunk)
    {
        if (trunk.TrunkIndex >= 0)
        {
            if (trunk.Ready)
                readyTrunks.Set(trunk.TrunkIndex);
            else
                readyTrunks.Reset(trunk.TrunkIndex);
        }

        if (trunkPile.Count == TrunkCount)
            FirstRegenerating = false;
    }

    internal void NotifyTrunkDeath(AxeMaxTrunk trunk)
    {
        if (trunk.Thrown)
            ThrownTrunkCount--;
        else
        {
            trunkPile.Remove(trunk);
            readyTrunks.Clear();

            for (int i = 0; i < trunkPile.Count; i++)
            {
                trunk = (AxeMaxTrunk) trunkPile[i];
                if (trunk != null)
                {
                    trunk.TrunkIndex = i;

                    if (trunk.Ready)
                        readyTrunks.Set(i);
                    else
                        readyTrunks.Reset(i);
                }
                else
                    readyTrunks.Reset(i);
            }
        }
    }
}