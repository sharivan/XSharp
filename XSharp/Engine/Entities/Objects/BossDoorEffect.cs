﻿using XSharp.Engine.Entities.Effects;
using XSharp.Engine.Graphics;

namespace XSharp.Engine.Entities.Objects;

internal class BossDoorEffect : SpriteEffect
{
    private EntityReference<BossDoor> door;

    public BossDoor Door
    {
        get => door;
        set => door = value;
    }

    public BossDoorState State
    {
        get => GetState<BossDoorState>();
        set => SetState(value);
    }

    public BossDoorEffect()
    {
    }

    protected internal override void OnCreate()
    {
        base.OnCreate();

        SpriteSheetName = "Boos Door";
        Directional = false;
        Respawnable = true;

        SetAnimationNames("Closed", "Opening", "PlayerCrossing", "Closing");

        SetupStateArray(typeof(BossDoorState));
        RegisterState(BossDoorState.CLOSED, OnStartClosed, "Closed");
        RegisterState(BossDoorState.OPENING, OnStartOpening, OnOpening, null, "Opening");
        RegisterState(BossDoorState.PLAYER_CROSSING, OnStartPlayerCrossing, OnPlayerCrossing, null, "PlayerCrossing");
        RegisterState(BossDoorState.CLOSING, OnStartClosing, null, OnEndClosing, "Closing");
    }

    private void OnStartClosed(EntityState state, EntityState lastState)
    {
        Door?.OnStartClosed();
    }

    private void OnStartOpening(EntityState state, EntityState lastState)
    {
        Door?.OnStartOpening();
    }

    private void OnOpening(EntityState state, long frameCounter)
    {
        Door?.OnOpening(frameCounter);
    }

    private void OnStartPlayerCrossing(EntityState state, EntityState lastState)
    {
        Door?.OnStartPlayerCrossing();
    }

    private void OnPlayerCrossing(EntityState state, long frameCounter)
    {
        Door?.OnPlayerCrossing(frameCounter);
    }

    private void OnStartClosing(EntityState state, EntityState lastState)
    {
        Door?.OnStartClosing();
    }

    private void OnEndClosing(EntityState state)
    {
        Door?.OnEndClosing();
    }

    protected override void OnDeath()
    {
        door = null;
        base.OnDeath();
    }

    protected internal override void OnAnimationEnd(Animation animation)
    {
        base.OnAnimationEnd(animation);

        switch (State)
        {
            case BossDoorState.OPENING when animation.Name == "Opening":
                State = BossDoorState.PLAYER_CROSSING;
                break;

            case BossDoorState.CLOSING when animation.Name == "Closing":
                State = BossDoorState.CLOSED;
                break;
        }
    }
}