using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using XSharp.Engine.Entities.Enemies;
using XSharp.Engine.World;
using XSharp.Geometry;
using static XSharp.Engine.Consts;
using static XSharp.Engine.World.World;

namespace XSharp.Engine.Entities
{
    public enum TouchingKind
    {
        VECTOR,
        BOX
    }

    public delegate void EntityEvent(Entity source);
    public delegate void EntityActivatorEvent(Entity source, Entity activator);

    public abstract class Entity
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

        internal string name = null;
        private Vector origin = Vector.NULL_VECTOR;
        internal Entity parent = null;

        internal readonly List<Entity> touchingEntities;
        internal readonly List<Entity> childs;
        private readonly HashSet<Entity> resultSet;

        internal Entity previous;
        internal Entity next;

        internal long frameToKill = -1;

        private readonly List<EntityState> states;
        private EntityState[] stateArray;
        private int currentStateID;

        private bool checkTouchingEntities = true;
        private bool checkTouchingWithDeadEntities = false;

        private BoxKind boxKind;
        private BoxKind lastBoxKind;
        private Box[] lastBox;

        public GameEngine Engine => GameEngine.Engine;

        public int Index
        {
            get;
            internal set;
        } = -1;

        public string Name
        {
            get => name;
            set => GameEngine.Engine.UpdateEntityName(this, value);
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

        public virtual Box Hitbox
        {
            get => Origin + (!Alive && Respawnable ? GetDeadBox() : GetHitbox());
            protected set
            {
                BeginUpdate();
                SetHitbox(value - value.Origin);
                SetOrigin(value.Origin);
                EndUpdate();
                UpdatePartition();
            }
        }

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
            private set;
        } = false;

        public int MinimumIntervalToRespawn
        {
            get;
            protected set;
        } = 4;

        public int MinimumIntervalToKillOnOffScreen
        {
            get;
            protected set;
        } = 4;

        public long SpawnFrame
        {
            get;
            private set;
        } = 0;

        public long DeathFrame
        {
            get;
            internal set;
        } = 0;

        public bool Updating
        {
            get;
            private set;
        } = false;

        public bool Spawning
        {
            get;
            private set;
        }

        public bool CheckTouchingEntities
        {
            get => checkTouchingEntities;
            protected set
            {
                checkTouchingEntities = value;
                UpdatePartition();
            }
        }

        public TouchingKind TouchingKind
        {
            get;
            set;
        } = TouchingKind.BOX;

        public VectorKind TouchingVectorKind
        {
            get;
            set;
        } = VectorKind.ORIGIN;

        public bool CheckTouchingWithDeadEntities
        {
            get => checkTouchingWithDeadEntities;
            protected set
            {
                checkTouchingWithDeadEntities = value;
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

        public bool KillOnOffscreen
        {
            get;
            protected set;
        } = false;

        protected Entity()
        {
            touchingEntities = new List<Entity>();
            childs = new List<Entity>();
            resultSet = new HashSet<Entity>();

            states = new List<EntityState>();

            lastBox = new Box[BOXKIND_COUNT];
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

        public virtual Vector GetVector(VectorKind kind)
        {
            return kind switch
            {
                VectorKind.ORIGIN => Origin,
                VectorKind.HITBOX_CENTER => Hitbox.Center,
                _ => Vector.NULL_VECTOR
            };
        }

        public virtual Vector GetLastVector(VectorKind kind)
        {
            return kind switch
            {
                VectorKind.ORIGIN => LastOrigin,
                VectorKind.HITBOX_CENTER => GetLastBox(BoxKind.HITBOX).Center,
                _ => Vector.NULL_VECTOR
            };
        }

        protected abstract Box GetHitbox();

        protected virtual Box GetDeadBox()
        {
            return GetHitbox();
        }

        protected virtual void SetHitbox(Box hitbox)
        {
        }

        public virtual Box GetBox(BoxKind kind)
        {
            return kind == BoxKind.HITBOX ? Hitbox : Box.EMPTY_BOX;
        }

        public Box GetLastBox(BoxKind kind)
        {
            return lastBox[kind.ToIndex()];
        }

        public virtual void LoadState(BinaryReader reader)
        {
            origin = new Vector(reader);
            LastOrigin = new Vector(reader);
            Alive = reader.ReadBoolean();
            MarkedToRemove = reader.ReadBoolean();
            Respawnable = reader.ReadBoolean();
            CheckTouchingEntities = reader.ReadBoolean();
        }

        public virtual void SaveState(BinaryWriter writer)
        {
            origin.Write(writer);
            LastOrigin.Write(writer);
            writer.Write(Alive);
            writer.Write(MarkedToRemove);
            writer.Write(Respawnable);
            writer.Write(CheckTouchingEntities);
        }

        public override string ToString()
        {
            return $"{GetType().Name}[{Name}, {Origin}]";
        }

        private bool CheckTouching(Entity entity)
        {
            if (TouchingKind == TouchingKind.VECTOR)
            {
                Vector v = entity.GetVector(TouchingVectorKind);
                return v <= Hitbox;
            }

            return true;
        }

        protected internal virtual void OnFrame()
        {
            if (frameToKill > 0 && Engine.FrameCounter >= frameToKill)
            {
                frameToKill = -1;
                Kill();
                return;
            }

            if (KillOnOffscreen && Engine.FrameCounter - SpawnFrame >= MinimumIntervalToKillOnOffScreen && IsOffscreen(VectorKind.ORIGIN))
            {
                Kill();
                return;
            }

            if (!Alive || MarkedToRemove)
                return;

            if (!PreThink())
                return;

            if (!Alive || MarkedToRemove)
                return;

            Think();

            if (!Alive || MarkedToRemove)
                return;

            CurrentState?.OnFrame();

            if (!Alive || MarkedToRemove)
                return;

            if (CheckTouchingEntities)
            {
                resultSet.Clear();
                Engine.partition.Query(resultSet, Hitbox, this, childs, BoxKind.HITBOX, !CheckTouchingWithDeadEntities);

                for (int i = 0; i < touchingEntities.Count; i++)
                {
                    Entity entity = touchingEntities[i];

                    if (!CheckTouching(entity) || !resultSet.Contains(entity))
                    {
                        touchingEntities.RemoveAt(i);
                        i--;
                        OnEndTouch(entity);
                    }
                    else if ((CheckTouchingWithDeadEntities || entity.Alive && !entity.MarkedToRemove) && CheckTouching(entity))
                    {
                        resultSet.Remove(entity);
                        OnTouching(entity);
                    }

                    if (!Alive || MarkedToRemove)
                        return;
                }

                foreach (Entity entity in resultSet)
                    if ((CheckTouchingWithDeadEntities || entity.Alive && !entity.MarkedToRemove) && CheckTouching(entity))
                    {
                        touchingEntities.Add(entity);
                        OnStartTouch(entity);

                        if (!Alive || MarkedToRemove)
                            return;
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

        public virtual void Kill()
        {
            if (!Alive || MarkedToRemove)
                return;

            MarkedToRemove = true;
            Engine.removedEntities.Add(this);

            OnDeath();
        }

        protected internal virtual void Cleanup()
        {
            foreach (Entity child in childs)
                child.parent = null;

            childs.Clear();
            touchingEntities.Clear();

            Parent = null;
            frameToKill = -1;
            currentStateID = -1;
            MarkedToRemove = false;
            Spawning = false;
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
            SpawnFrame = Engine.FrameCounter;
            boxKind = BoxKind.NONE;
            lastBoxKind = BoxKind.NONE;
            Spawning = true;
            CheckTouchingEntities = true;
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

            UpdatePartition(true);
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

        protected virtual BoxKind ComputeBoxKind()
        {
            return Respawnable || CheckTouchingEntities ? BoxKind.HITBOX : BoxKind.NONE;
        }

        public void BeginUpdate()
        {
            Updating = true;
        }

        public void EndUpdate()
        {
            Updating = false;
        }

        protected internal virtual void UpdatePartition(bool force = false)
        {
            if (Updating || !Respawnable && !Alive)
                return;

            boxKind = ComputeBoxKind();

            if (lastBoxKind == BoxKind.NONE && boxKind != BoxKind.NONE)
                Engine.partition.Insert(this, boxKind);
            else if (lastBoxKind != BoxKind.NONE && boxKind == BoxKind.NONE)
                Engine.partition.Remove(this, boxKind);
            else if (lastBoxKind != boxKind)
            {
                for (int i = 0; i < BOXKIND_COUNT; i++)
                {
                    var k = i.ToBoxKind();
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
            UpdateLastBoxes();
        }

        private void UpdateLastBoxes()
        {
            for (int i = 0; i < BOXKIND_COUNT; i++)
            {
                var kind = i.ToBoxKind();
                lastBox[i] = GetBox(kind);
            }
        }

        public bool IsOffscreen(VectorKind kind, bool extendedCamera = true)
        {
            return GetVector(kind) > (extendedCamera ? Engine.World.Camera.ExtendedBoundingBox : Engine.World.Camera.BoundingBox);
        }

        public bool IsOffscreen(BoxKind kind, bool extendedCamera = true)
        {
            return !CollisionChecker.HasIntersection(GetBox(kind), extendedCamera ? Engine.World.Camera.ExtendedBoundingBox : Engine.World.Camera.BoundingBox);
        }

        public virtual void Place()
        {
            Respawnable = true;

            if (!Engine.respawnableEntities.ContainsKey(this))
                Engine.respawnableEntities.Add(this, new RespawnEntry(this, Origin));

            UpdatePartition(true);
        }

        public virtual void Unplace()
        {
            Respawnable = false;

            if (Engine.respawnableEntities.ContainsKey(this))
                Engine.respawnableEntities.Remove(this);

            UpdatePartition(true);
        }
    }
}