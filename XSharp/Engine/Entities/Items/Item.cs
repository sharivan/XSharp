using static XSharp.Engine.Consts;

namespace XSharp.Engine.Entities.Items;

public abstract class Item : Sprite
{
    public int DurationFrames
    {
        get;
        set;
    } = ITEM_DURATION_FRAMES;

    public int DurationFrameCounter
    {
        get;
        set;
    } = 0;

    public bool Collected
    {
        get;
        private set;
    } = false;

    protected Item()
    {
        Directional = false;
    }

    protected internal override void OnSpawn()
    {
        base.OnSpawn();

        DurationFrameCounter = 0;
        KillOnOffscreen = true;
    }

    protected virtual void OnCollecting(Player player)
    {
    }

    protected override void OnStartTouch(Entity entity)
    {
        base.OnStartTouch(entity);

        if (entity is Player player)
        {
            Collected = true;
            OnCollecting(player);
            Kill();
        }
    }

    protected override void Think()
    {
        base.Think();

        if (DurationFrames > 0)
        {
            if (DurationFrameCounter >= DurationFrames)
            {
                Kill();
                return;
            }

            if (!Blinking && DurationFrameCounter >= DurationFrames - ITEM_BLINKING_FRAMES)
                Blinking = true;
        }

        if (Landed || BlockedLeft || BlockedRight || BlockedUp)
            DurationFrameCounter++;
    }
}