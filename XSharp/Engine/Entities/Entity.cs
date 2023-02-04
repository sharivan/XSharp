using System;
using System.Collections.Generic;
using System.IO;
using XSharp.Geometry;
using static XSharp.Engine.Consts;
using static XSharp.Engine.World.World;

namespace XSharp.Engine.Entities
{
    public enum VectorKind
    {
        NONE = 0,
        ORIGIN = 1,
        BOUDINGBOX_CENTER = 2,
        HITBOX_CENTER = 4,
        ALL = 255
    }

    public enum BoxKind
    {
        NONE = 0,
        BOUDINGBOX = 1,
        HITBOX = 2,
        ALL = 255
    }

    public delegate void EntityEvent(Entity source);
    public delegate void EntityActivatorEvent(Entity source, Entity activator);

    public abstract class Entity : IDisposable
    {
        public event EntityEvent SpawnEvent;
        public event EntityEvent DeathEvent;
        public event EntityEvent VisibleChangedEvent;
        public event EntityEvent PreThinkEvent;
        public event EntityEvent ThinkEvent;
        public event EntityEvent PostThinkEvent;
        public event EntityActivatorEvent StartTouchEvent;
        public event EntityActivatorEvent TouchingEvent;
        public event EntityActivatorEvent EndTouchEvent;

        private string name;
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

        private bool visible;
        private bool checkCollisionWithEntities;

        private BoxKind boxKind;
        private BoxKind lastBoxKind;

        public GameEngine Engine => GameEngine.Engine;

        public int Index
        {
            get;
            internal set;
        }

        public string Name
        {
            get => name;
            set => name = value ?? Engine.GetExclusiveName(GetType().Name);
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

        public Vector Origin
        {
            get => GetOrigin();
            set => SetOrigin(value);
        }

        public Vector LastOrigin
        {
            get;
            private set;
        }

        public Box BoundingBox
        {
            get => Origin + GetBoundingBox();
            set
            {
                SetBoundingBox(value - value.Origin);
                SetOrigin(value.Origin);
            }
        }

        public Box LastBoundingBox => LastOrigin + GetBoundingBox();

        public Box Hitbox
        {
            get => Origin + GetHitbox();
            set
            {
                SetHitbox(value - value.Origin);
                SetOrigin(value.Origin);
            }
        }

        public Box LastHitBox => LastOrigin + GetHitbox();

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

        public bool Visible
        {
            get => visible;
            set
            {
                visible = value;
                UpdatePartition();
            }
        }

        public bool Spawning
        {
            get;
            private set;
        }

        public bool Offscreen => !HasIntersection(BoundingBox, Engine.World.Camera.ExtendedBoundingBox);

        public bool CheckCollisionWithEntities
        {
            get => checkCollisionWithEntities;
            set
            {
                checkCollisionWithEntities = value;
                UpdatePartition();
            }
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

        protected Entity(string name, Vector origin)
        {
            Name = name;
            this.origin = origin;

            touchingEntities = new List<Entity>();
            childs = new List<Entity>();

            states = new List<EntityState>();
        }

        protected Entity(Vector origin) : this(null, origin)
        {
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

        protected EntityState RegisterState(int id, EntityStateFrameEvent onFrame)
        {
            return RegisterState(id, null, onFrame, null);
        }

        protected EntityState RegisterState(int id)
        {
            return RegisterState(id, null, null, null);
        }

        protected EntityState RegisterState<T>(T id, EntityStateEvent onStart, EntityStateFrameEvent onFrame, EntityStateEvent onEnd) where T : Enum
        {
            return RegisterState((int) (object) id, onStart, onFrame, onEnd);
        }

        protected EntityState RegisterState<T>(T id, EntityStateFrameEvent onFrame) where T : Enum
        {
            return RegisterState((int) (object) id, null, onFrame, null);
        }

        protected EntityState RegisterState<T>(T id) where T : Enum
        {
            return RegisterState((int) (object) id, null, null, null);
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

        protected virtual Vector GetOrigin()
        {
            return origin;
        }

        protected virtual void SetOrigin(Vector origin)
        {
            LastOrigin = this.origin;
            this.origin = origin;

            UpdatePartition();

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
                VectorKind.ORIGIN => Origin,
                VectorKind.BOUDINGBOX_CENTER => BoundingBox.Center,
                VectorKind.HITBOX_CENTER => Hitbox.Center,
                _ => Vector.NULL_VECTOR
            };
        }

        public Vector GetLastVector(VectorKind kind)
        {
            return kind switch
            {
                VectorKind.ORIGIN => LastOrigin,
                VectorKind.BOUDINGBOX_CENTER => LastBoundingBox.Center,
                VectorKind.HITBOX_CENTER => LastHitBox.Center,
                _ => Vector.NULL_VECTOR
            };
        }

        protected abstract Box GetBoundingBox();

        protected virtual Box GetHitbox()
        {
            return GetBoundingBox();
        }

        protected virtual void SetBoundingBox(Box boudingBox)
        {
        }

        protected virtual void SetHitbox(Box hitbox)
        {
        }

        public Box GetBox(BoxKind kind)
        {
            return kind switch
            {
                BoxKind.BOUDINGBOX => BoundingBox,
                BoxKind.HITBOX => Hitbox,
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
                List<Entity> touching = Engine.partition.Query(Hitbox, this, childs, BoxKind.HITBOX);

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
            StartTouchEvent?.Invoke(this, entity);
        }

        protected virtual void OnTouching(Entity entity)
        {
            TouchingEvent?.Invoke(this, entity);
        }

        protected virtual void OnEndTouch(Entity entity)
        {
            EndTouchEvent?.Invoke(this, entity);
        }

        protected virtual bool PreThink()
        {
            PreThinkEvent?.Invoke(this);
            return true;
        }

        protected virtual void Think()
        {
            ThinkEvent?.Invoke(this);
        }

        protected internal virtual void PostThink()
        {
            PostThinkEvent?.Invoke(this);
        }

        public virtual void Dispose()
        {
        }

        public virtual void Kill()
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
            boxKind = BoxKind.NONE;
            lastBoxKind = BoxKind.NONE;
            Spawning = true;
            Visible = false;
            CheckCollisionWithEntities = true;
            frameToKill = -1;
            currentStateID = -1;
            MarkedToRemove = false;
            Engine.addedEntities.Add(this);
        }

        protected internal virtual void OnSpawn()
        {
            SpawnEvent?.Invoke(this);
        }

        protected internal virtual void PostSpawn()
        {
            Spawning = false;
            Alive = true;

            UpdatePartition();
        }

        protected virtual void OnDeath()
        {
            DeathEvent?.Invoke(this);
        }

        protected virtual void OnVisible()
        {
            // TODO : Implement call to this and implement OnInvisible()
            VisibleChangedEvent?.Invoke(this);
        }

        protected virtual void UpdatePartition(bool force = false)
        {
            if (!Alive)
                return;

            boxKind = (Visible ? BoxKind.BOUDINGBOX : BoxKind.NONE) | (CheckCollisionWithEntities ? BoxKind.HITBOX : BoxKind.NONE);

            if (lastBoxKind == BoxKind.NONE && boxKind != BoxKind.NONE)
                Engine.partition.Insert(this, boxKind);
            else if (lastBoxKind != BoxKind.NONE && boxKind == BoxKind.NONE)
                Engine.partition.Remove(this, boxKind);
            else if (lastBoxKind != boxKind)
            {
                for (int i = 0; i < BOXKIND_COUNT; i++)
                {
                    var k = (BoxKind) (1 << i);
                    if (boxKind.HasFlag(k) && !lastBoxKind.HasFlag(k))
                        Engine.partition.Insert(this, k);
                    else if (!boxKind.HasFlag(k) && lastBoxKind.HasFlag(k))
                        Engine.partition.Remove(this, k);
                    else
                        Engine.partition.Update(this, k, force);
                }
            }
            else
                Engine.partition.Update(this, boxKind, force);

            lastBoxKind = boxKind;
        }
    }
}