﻿using XSharp.Engine.Entities.Triggers;
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

        public BossDoorDirection CrossDirection
        {
            get;
            set;
        }

        public bool Bidirectional
        {
            get;
            set;
        }

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

        public BossDoor(string name, Vector origin)
        {
            Hitbox = GetTriggerBoudingBox(origin);

            effect = new BossDoorEffect()
            {
                Door = this,
                Origin = origin,
                Visible = false
            };
        }

        public BossDoor(Vector origin) : this(null, origin)
        {
        }

        protected internal override void OnSpawn()
        {
            base.OnSpawn();

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

        internal void OnStartPlayerCrossing()
        {
            Engine.Player.Animating = true;

            Cell sceneCell = GetSceneCellFromPos(Engine.Player.Origin);
            Box sceneBox = GetSceneBoundingBox(sceneCell);
            Engine.World.Camera.MoveToLeftTop(sceneBox.LeftTop + (SCENE_SIZE, 0), 1.5);

            if (StartBossBattle)
            {
                Engine.CameraConstraintsOrigin = Engine.CurrentCheckpoint.Origin + (SCENE_SIZE, 0);
                Engine.CameraConstraintsBox = Engine.CurrentCheckpoint.Hitbox + (SCENE_SIZE, 0);
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
                Engine.Player.Invincible = false;
                Engine.Player.InputLocked = false;
            }
        }
    }
}