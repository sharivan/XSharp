using MMX.Geometry;

using static MMX.Engine.Consts;

namespace MMX.Engine.Entities.Items
{
    public class Item : Sprite
    {
        public int DurationFrames
        {
            get;
            set;
        }

        public int DurationFrameCounter
        {
            get;
            set;
        }

        public bool Collected
        {
            get;
            private set;
        } = false;

        public Item(GameEngine engine, string name, Vector origin, int durationFrames, int spriteSheetIndex, string[] animationNames = null, string initialAnimationName = null, bool directional = false) : base(engine, name, origin, spriteSheetIndex, animationNames, initialAnimationName, directional)
        {
            DurationFrames = durationFrames;
        }

        public Item(GameEngine engine, string name, Vector origin, int durationFrames, int spriteSheetIndex, bool directional = false, params string[] animationNames) : base(engine, name, origin, spriteSheetIndex, directional, animationNames)
        {
            DurationFrames = durationFrames;
        }

        protected internal override void OnSpawn()
        {
            base.OnSpawn();

            DurationFrameCounter = 0;
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
                if (Offscreen || DurationFrameCounter >= DurationFrames)
                    Kill();

                if (!Blinking && DurationFrameCounter >= DurationFrames - ITEM_BLINKING_FRAMES)
                    Blinking = true;
            }

            if (Landed || BlockedLeft || BlockedRight || BlockedUp)
                DurationFrameCounter++;
        }
    }
}