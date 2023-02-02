using MMX.Geometry;
using System;
using System.Collections.Generic;
using System.IO;
using static MMX.Engine.World.World;

namespace MMX.Engine.Entities
{
    public enum VectorKind
    {
        NONE = 0,
        ORIGIN = 1,
        PLAYER_ORIGIN = 2,
        BOUDINGBOX_CENTER = 4,
        HITBOX_CENTER = 8,
        ALL = 255
    }

    public enum BoxKind
    {
        NONE = 0,
        BOUDINGBOX = 1,
        HITBOX = 2,
        ALL = 255
    }

    public abstract class Entity : IDisposable
    {
        private Vector origin;
        internal Entity parent;
        private readonly List<Entity> touchingEntities;
        internal readonly List<Entity> childs;

        internal Entity previous;
        internal Entity next;

        internal long frameToKill = -1;

        private readonly List<EntityState> states;
        private EntityState[] stateArray;
        private int currentStateID;

        public GameEngine Engine
        {
            get;
        }

        public int Index
        {
            get;
            internal set;
        }

        public string Name
        {
            get;
            set;
        }

        public Vector Origin
        {
            get => origin;
            set => SetOrigin(value);
        }

        public Entity Parent
        {
            get => parent;

            set
            {
                if (parent == value)
                    return;

                if (value != null && value.IsParent(this))
                    throw new ArgumentException("Cyclic parenting is not allowed.");

                parent?.childs.Remove(this);
                parent = value;
                parent?.childs.Add(this);
            }
        }

        public IEnumerable<Entity> TouchingEntities => touchingEntities;

        public IEnumerable<Entity> Childs => childs;

        public Vector LastOrigin
        {
            get;
            private set;
        }

        public Box BoundingBox
        {
            get => GetBoundingBox();
            set => SetBoundingBox(value);
        }

        public Box HitBox => GetHitBox();

        public bool Alive
        {
            get;
            internal set;
        }

        public bool MarkedToRemove
        {
            get;
            private set;
        }

        public bool Respawnable
        {
            get;
            set;
        }

        public bool Offscreen => !HasIntersection(BoundingBox, Engine.World.Camera.ExtendedBoundingBox);

        public bool CheckCollisionWithEntities
        {
            get;
            set;
        }

        public int StateCount => states.Count;

        public int CurrentStateID
        {
            get => currentStateID;
            set
            {
                if (currentStateID != value)
                {
                    EntityState state = CurrentState;
                    state?.OnEnd();

                    currentStateID = value;

                    state = CurrentState;
                    state?.OnStart();
                }
            }
        }

        protected EntityState CurrentState => stateArray != null && CurrentStateID >= 0 ? stateArray[CurrentStateID] : null;

        protected Entity(GameEngine engine, string name, Vector origin)
        {
            Engine = engine;
            Name = name;
            this.origin = origin;

            touchingEntities = new List<Entity>();
            childs = new List<Entity>();

            states = new List<EntityState>();
        }

        protected void SetupStateArray(int count)
        {
            states.Clear();
            stateArray = new EntityState[count];
        }

        protected void SetupStateArray(Type t)
        {
            SetupStateArray(Enum.GetNames(t).Length);
        }

        protected virtual Type GetStateType()
        {
            return typeof(EntityState);
        }

        protected virtual void OnRegisterState(EntityState state)
        {
        }

        protected EntityState RegisterState(int id, EntityStateEvent onStart, EntityStateFrameEvent onFrame, EntityStateEvent onEnd)
        {
            Type stateType = GetStateType();
            var state = (EntityState) Activator.CreateInstance(stateType);

            state.Entity = this;
            state.ID = id;
            state.StartEvent += onStart;
            state.FrameEvent += onFrame;
            state.EndEvent += onEnd;

            states.Add(state);
            stateArray[id] = state;
            OnRegisterState(state);

            return state;
        }

        protected EntityState RegisterState<T>(T id, EntityStateEvent onStart, EntityStateFrameEvent onFrame, EntityStateEvent onEnd) where T : Enum
        {
            return RegisterState((int) (object) id, onStart, onFrame, onEnd);
        }

        protected EntityState RegisterState(int id, EntityStateFrameEvent onFrame)
        {
            return RegisterState(id, null, onFrame, null);
        }

        protected EntityState RegisterState<T>(T id, EntityStateFrameEvent onFrame) where T : Enum
        {
            return RegisterState((int) (object) id, null, onFrame, null);
        }

        protected void UnregisterState(int id)
        {
            EntityState state = stateArray[id];
            stateArray[id] = null;
            states.Remove(state);
        }

        protected EntityState GetStateByID(int id)
        {
            return stateArray[id];
        }

        protected internal T GetState<T>() where T : Enum
        {
            return (T) (object) CurrentStateID;
        }

        protected internal void SetState<T>(T id) where T : Enum
        {
            CurrentStateID = (int) (object) id;
        }

        public bool IsTouching(Entity other)
        {
            return touchingEntities.Contains(other);
        }

        public bool Contains(Entity other)
        {
            return childs.Contains(other);
        }

        protected virtual void SetOrigin(Vector origin)
        {
            LastOrigin = this.origin;
            this.origin = origin;
            Engine.partition.Update(this);

            Vector delta = origin - LastOrigin;
            foreach (Entity child in childs)
                child.Origin += delta;
        }

        public bool IsParent(Entity entity)
        {
            if (entity == null)
                return false;

            Entity parent = this.parent;
            while (parent != null)
            {
                if (parent == entity)
                    return true;

                parent = parent.parent;
            }

            return false;
        }

        public Vector GetVector(VectorKind kind)
        {
            return kind switch
            {
                VectorKind.ORIGIN => origin,
                VectorKind.PLAYER_ORIGIN => origin - new Vector(0, 17),
                VectorKind.BOUDINGBOX_CENTER => BoundingBox.Center,
                VectorKind.HITBOX_CENTER => HitBox.Center,
                _ => Vector.NULL_VECTOR
            };
        }

        protected abstract Box GetBoundingBox();

        protected virtual Box GetHitBox()
        {
            return GetBoundingBox();
        }

        protected virtual void SetBoundingBox(Box boudingBox)
        {
        }

        public Box GetBox(BoxKind kind)
        {
            return kind switch
            {
                BoxKind.BOUDINGBOX => BoundingBox,
                BoxKind.HITBOX => HitBox,
                _ => Box.EMPTY_BOX,
            };
        }

        public virtual void LoadState(BinaryReader reader)
        {
            origin = new Vector(reader);
            LastOrigin = new Vector(reader);
            Alive = reader.ReadBoolean();
            MarkedToRemove = reader.ReadBoolean();
            Respawnable = reader.ReadBoolean();
            CheckCollisionWithEntities = reader.ReadBoolean();
        }

        public virtual void SaveState(BinaryWriter writer)
        {
            origin.Write(writer);
            LastOrigin.Write(writer);
            writer.Write(Alive);
            writer.Write(MarkedToRemove);
            writer.Write(Respawnable);
            writer.Write(CheckCollisionWithEntities);
        }

        public override string ToString()
        {
            return $"{GetType().Name}[{Name}, {Origin}]";
        }

        protected internal virtual void OnFrame()
        {
            if (frameToKill > 0 && Engine.FrameCounter >= frameToKill)
            {
                frameToKill = -1;
                Kill();
                return;
            }

            if (!Alive)
                return;

            if (!PreThink())
                return;

            Think();
            CurrentState?.OnFrame();

            if (CheckCollisionWithEntities)
            {
                List<Entity> touching = Engine.partition.Query(HitBox, this, childs, BoxKind.HITBOX);

                int count = touchingEntities.Count;
                for (int i = 0; i < count; i++)
                {
                    Entity entity = touchingEntities[i];
                    int index = touching.IndexOf(entity);

                    if (index == -1)
                    {
                        touchingEntities.RemoveAt(i);
                        i--;
                        count--;
                        OnEndTouch(entity);
                    }
                    else if (entity.Alive && !entity.MarkedToRemove)
                    {
                        touching.RemoveAt(index);
                        OnTouching(entity);
                    }
                }

                foreach (Entity entity in touching)
                    if (entity.Alive && !entity.MarkedToRemove)
                    {
                        touchingEntities.Add(entity);
                        OnStartTouch(entity);
                    }
            }
        }

        protected virtual void OnStartTouch(Entity entity)
        {
        }

        protected virtual void OnTouching(Entity entity)
        {
        }

        protected virtual void OnEndTouch(Entity entity)
        {
        }

        protected virtual bool PreThink()
        {
            return true;
        }

        protected virtual void Think()
        {
        }

        protected internal virtual void PostThink()
        {
        }

        public virtual void Dispose()
        {
        }

        public void Kill()
        {
            if (!Alive || MarkedToRemove)
                return;

            MarkedToRemove = true;
            Engine.removedEntities.Add(this);
            Engine.partition.Remove(this);

            OnDeath();
        }

        public void KillOnNextFrame()
        {
            KillOnFrame(Engine.FrameCounter + 1);
        }

        public void KillOnFrame(long frameNumber)
        {
            frameToKill = frameNumber;
        }

        public virtual void Spawn()
        {
            CheckCollisionWithEntities = true;
            frameToKill = -1;
            currentStateID = -1;
            Alive = true;
            MarkedToRemove = false;
            Engine.addedEntities.Add(this);
        }

        protected internal virtual void OnSpawn()
        {           
        }

        protected virtual void OnDeath()
        {
        }

        protected virtual void OnVisible()
        {

        }
    }
}