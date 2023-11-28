﻿using XSharp.Engine.Graphics;
using XSharp.Math;
using XSharp.Math.Geometry;

namespace XSharp.Engine.Entities.Enemies.Bosses.Sigma;

public class JediSigmaVerticalBeamSaber : Enemy
{
    #region Precache
    [Precache]
    new internal static void Precache()
    {
        Engine.CallPrecacheAction<JediSigma>();
    }
    #endregion

    public JediSigmaVerticalBeamSaber()
    {
    }

    protected override void OnCreate()
    {
        base.OnCreate();

        DefaultDirection = Direction.LEFT;
        ContactDamage = JediSigma.SHOT_DAMAGE;

        PaletteName = "JediSigmaPalette";
        SpriteSheetName = "JediSigma";

        SetAnimationNames("VerticalBeamSaber");
        InitialAnimationName = "VerticalBeamSaber";
    }

    public override FixedSingle GetGravity()
    {
        return 0;
    }

    protected override Box GetHitbox()
    {
        return JediSigma.SHOT_HITBOX;
    }

    protected override void OnSpawn()
    {
        base.OnSpawn();
    }
}