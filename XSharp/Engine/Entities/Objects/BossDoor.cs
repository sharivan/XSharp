using XSharp.Engine.Entities.Triggers;
using XSharp.Engine.World;
using XSharp.Geometry;

using static XSharp.Engine.Consts;
using static XSharp.Engine.World.World;

namespace XSharp.Engine.Entities.Objects
{
    public enum BossDoorState
    {
        CLOSED = 0,
        OPENING = 1,
        PLAYER_CROSSING = 2,
        CLOSING = 3
    }

    public enum BossDoorOrientation
    {
        VERTICAL,
        HORIZONTAL
    }

    public enum BossDoorDirection
    {
        FORWARD,
        BACKWARD
    }

    public delegate void BossDoorEvent(BossDoor source);

    public class BossDoor : BaseTrigger
    {
        private static Box GetTriggerBoudingBox(Vector origin)
        {
            return (origin, (-13, -8), (8, 8));
        }

        public event BossDoorEvent OpeningEvent;
        public event BossDoorEvent PlayerCrossingEvent;
        public event BossDoorEvent ClosingEvent;
        public event BossDoorEvent ClosedEvent;

        private readonly BossDoorEffect effect;

        public BossDoorOrientation Orientation
        {
            get;
            set;
        } = BossDoorOrientation.VERTICAL;

        public BossDoorDirection CrossDirection
        {
            get;
            set;
        } = BossDoorDirection.FORWARD;

        public bool Bidirectional
        {
            get;
            set;
        } = false;

        public BossDoorState State
        {
            get => effect.State;
            set => effect.State = value;
        }

        public bool StartBossBattle
        {
            get;
            set;
        }

        public BossDoor()
        {
            effect = new BossDoorEffect()
            {
                Door = this,
                Visible = false
            };
        }

        protected internal override void OnSpawn()
        {
            base.OnSpawn();

            Hitbox = GetTriggerBoudingBox(Origin);

            effect.Origin = (Origin.X, Origin.Y - 1);
            effect.Spawn();

            State = BossDoorState.CLOSED;
        }

        protected override void OnDeath()
        {
            effect.Kill();

            base.OnDeath();
        }

        protected override void OnStartTrigger(Entity entity)
        {
            base.OnStartTrigger(entity);

            if (entity is Player player)
            {
                Engine.World.Camera.NoConstraints = true;
                Engine.World.Camera.FocusOn = null;
                player.StartBossDoorCrossing();
                Engine.KillAllAliveEnemiesAndWeapons();

                if (player.ChargingEffect != null)
                    Engine.FreezeSprites(player, effect, player.ChargingEffect);
                else
                    Engine.FreezeSprites(player, effect);

                Engine.Player.Animating = false;
                Engine.Player.InputLocked = true;

                if (!Bidirectional)
                    Enabled = false;

                effect.Visible = true;
                State = BossDoorState.OPENING;
            }
        }

        internal void OnStartClosed()
        {
            effect.Visible = false;
            ClosedEvent?.Invoke(this);
        }

        internal void OnStartOpening()
        {
            OpeningEvent?.Invoke(this);
        }

        internal void OnOpening(long frameCounter)
        {
            if (frameCounter == 44)
                Engine.PlaySound(0, 22, true);
        }

        private Vector GetCameraMoveOffset()
        {
            switch (Orientation)
            {
                case BossDoorOrientation.VERTICAL:
                    if (CrossDirection == BossDoorDirection.FORWARD)
                        return (SCREEN_WIDTH, 0);

                    return (-SCREEN_WIDTH, 0);

                case BossDoorOrientation.HORIZONTAL:
                    if (CrossDirection == BossDoorDirection.FORWARD)
                        return (0, SCREEN_HEIGHT);

                    return (0, -SCREEN_HEIGHT);
            }

            return Vector.NULL_VECTOR;
        }

        internal void OnStartPlayerCrossing()
        {
            Engine.Player.Animating = true;

            Cell sceneCell = GetSceneCellFromPos(Engine.Player.Origin);
            Box sceneBox = GetSceneBoundingBox(sceneCell);
            Vector offset = GetCameraMoveOffset();
            Engine.World.Camera.MoveToLeftTop(sceneBox.LeftTop + offset, CAMERA_SMOOTH_SPEED);

            if (StartBossBattle)
            {
                Engine.CameraConstraintsOrigin = Engine.CurrentCheckpoint.Origin + offset;
                Engine.CameraConstraintsBox = Engine.CurrentCheckpoint.Hitbox + offset;
            }

            PlayerCrossingEvent?.Invoke(this);
        }

        internal void OnPlayerCrossing(long frameCounter)
        {
            if (frameCounter == 120)
            {
                Engine.Player.StopBossDoorCrossing();
                Engine.UnfreezeSprites();
                Engine.World.Camera.NoConstraints = false;
                Engine.World.Camera.FocusOn = Engine.Player;
                State = BossDoorState.CLOSING;
            }
            else if (frameCounter < 120)
                Engine.Player.Velocity = (CROSSING_BOOS_DOOR_SPEED, 0);
        }

        internal void OnStartClosing()
        {
            Engine.PlaySound(0, StartBossBattle ? 23 : 22, true);
            ClosingEvent?.Invoke(this);
        }

        internal void OnEndClosing()
        {
            if (StartBossBattle)
                Engine.StartBossBattle();
            else
            {
                var player = Engine.Player;
                player.Invincible = false;
                player.Blinking = false;
                player.InputLocked = false;
            }
        }
    }
}