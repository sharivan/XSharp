using System;
using System.Collections.Generic;
using System.IO;
using XSharp.Engine.World;
using XSharp.Geometry;
using static XSharp.Engine.Consts;

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

        private bool wasOffScreen;

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

        public static GameEngine Engine => GameEngine.Engine;

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
                SetHitbox(value - Origin);
                UpdatePartition();
            }
        }

        public virtual Box TouchingBox => Origin + GetTouchingBox();

        public bool Alive
        {
            get;
            internal set;
        }

        public bool Dead
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
        } = false;

        public bool RespawnOnNear
        {
            get;
            set;
        } = false;

        public int MinimumIntervalToRespawn
        {
            get;
            set;
        } = 4;

        public int MinimumIntervalToKillOnOffScreen
        {
            get;
            set;
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
            set
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
            set
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
                    EntityState lastState = CurrentState;
                    lastState?.OnEnd();

                    currentStateID = value;

                    EntityState currentState = CurrentState;
                    currentState?.OnStart(lastState);
                }
            }
        }

        protected EntityState CurrentState => stateArray != null && CurrentStateID >= 0 ? stateArray[CurrentStateID] : null;

        public bool KillOnOffscreen
        {
            get;
            set;
        } = false;

        protected Entity()
        {
            touchingEntities = new List<Entity>();
            childs = new List<Entity>();
            resultSet = new HashSet<Entity>();

            states = new List<EntityState>();

            lastBox = new Box[BOXKIND_COUNT];
        }

        protected internal virtual void ReadInitParams(dynamic initParams)
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

        protected void SetupStateArray<T>() where T : Enum
        {
            SetupStateArray(typeof(T));
        }

        protected virtual Type GetStateType()
        {
            return typeof(EntityState);
        }

        protected virtual void OnRegisterState(EntityState state)
        {
        }

        protected virtual void OnRegisterSubState(EntitySubState subState)
        {
        }

        protected EntityState RegisterState(int id, EntityStateStartEvent onStart, EntityStateFrameEvent onFrame, EntityStateEndEvent onEnd, int subStateCount)
        {
            Type stateType = GetStateType();
            var state = (EntityState) Activator.CreateInstance(stateType);

            state.Entity = this;
            state.ID = id;
            state.StartEvent += onStart;
            state.FrameEvent += onFrame;
            state.EndEvent += onEnd;

            if (subStateCount > 0)
                state.InitializeSubStates(subStateCount);

            states.Add(state);
            stateArray[id] = state;
            OnRegisterState(state);

            return state;
        }

        protected EntityState RegisterState(int id, EntityStateStartEvent onStart, int subStateCount)
        {
            return RegisterState(id, onStart, null, null, subStateCount);
        }

        protected EntityState RegisterState(int id, EntityStateFrameEvent onFrame, int subStateCount)
        {
            return RegisterState(id, null, onFrame, null, subStateCount);
        }

        protected EntityState RegisterState(int id, EntityStateEndEvent onEnd, int subStateCount)
        {
            return RegisterState(id, null, null, onEnd, subStateCount);
        }

        protected EntityState RegisterState(int id, int subStateCount)
        {
            return RegisterState(id, null, null, null, subStateCount);
        }

        protected EntityState RegisterState<T>(T id, EntityStateStartEvent onStart, EntityStateFrameEvent onFrame, EntityStateEndEvent onEnd) where T : Enum
        {
            return RegisterState((int) (object) id, onStart, onFrame, onEnd, 0);
        }

        protected EntityState RegisterState<T>(T id, EntityStateStartEvent onStart) where T : Enum
        {
            return RegisterState((int) (object) id, onStart, null, null, 0);
        }

        protected EntityState RegisterState<T>(T id, EntityStateFrameEvent onFrame) where T : Enum
        {
            return RegisterState((int) (object) id, null, onFrame, null, 0);
        }

        protected EntityState RegisterState<T>(T id, EntityStateEndEvent onEnd) where T : Enum
        {
            return RegisterState((int) (object) id, null, null, onEnd, 0);
        }

        protected EntityState RegisterState<T>(T id) where T : Enum
        {
            return RegisterState((int) (object) id, null, null, null, 0);
        }

        protected EntityState RegisterState<T, U>(T id, EntityStateStartEvent onStart, EntityStateFrameEvent onFrame, EntityStateEndEvent onEnd) where T : Enum where U : Enum
        {
            return RegisterState((int) (object) id, onStart, onFrame, onEnd, Enum.GetNames(typeof(U)).Length);
        }

        protected EntityState RegisterState<T, U>(T id, EntityStateStartEvent onStart) where T : Enum where U : Enum
        {
            return RegisterState((int) (object) id, onStart, null, null, Enum.GetNames(typeof(U)).Length);
        }

        protected EntityState RegisterState<T, U>(T id, EntityStateFrameEvent onFrame) where T : Enum where U : Enum
        {
            return RegisterState((int) (object) id, null, onFrame, null, Enum.GetNames(typeof(U)).Length);
        }

        protected EntityState RegisterState<T, U>(T id, EntityStateEndEvent onEnd) where T : Enum where U : Enum
        {
            return RegisterState((int) (object) id, null, null, onEnd, Enum.GetNames(typeof(U)).Length);
        }

        protected EntityState RegisterState<T, U>(T id) where T : Enum where U : Enum
        {
            return RegisterState((int) (object) id, null, null, null, Enum.GetNames(typeof(U)).Length);
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
            this.origin = origin.TruncFracPart();

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

        protected virtual Box GetTouchingBox()
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

        protected virtual bool CheckTouching(Entity entity)
        {
            if (TouchingKind == TouchingKind.VECTOR)
            {
                Vector v = entity.GetVector(TouchingVectorKind);
                return v <= Hitbox;
            }

            return (Hitbox & entity.Hitbox).IsValid();
        }

        protected internal virtual void OnFrame()
        {
            if (frameToKill > 0 && Engine.FrameCounter >= frameToKill)
            {
                frameToKill = -1;
                Kill();
                return;
            }

            bool offScreen = IsOffscreen(VectorKind.ORIGIN);
            if (offScreen && !wasOffScreen)
                OnOffScreen();

            wasOffScreen = offScreen;

            if (KillOnOffscreen && Engine.FrameCounter - SpawnFrame >= MinimumIntervalToKillOnOffScreen && offScreen)
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

            if (CheckTouchingEntities)
            {
                resultSet.Clear();
                Engine.partition.Query(resultSet, TouchingBox, this, childs, BoxKind.HITBOX, !CheckTouchingWithDeadEntities);

                for (int i = 0; i < touchingEntities.Count; i++)
                {
                    Entity entity = touchingEntities[i];

                    bool touching = CheckTouching(entity);

                    if (!touching || !resultSet.Contains(entity))
                    {
                        touchingEntities.RemoveAt(i);
                        i--;
                        OnEndTouch(entity);
                    }
                    else if (touching && (CheckTouchingWithDeadEntities || entity.Alive && !entity.MarkedToRemove))
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
                        if (!Alive || MarkedToRemove)
                            return;

                        touchingEntities.Add(entity);
                        OnStartTouch(entity);

                        if (!Alive || MarkedToRemove)
                            return;
                    }
            }

            if (!Alive || MarkedToRemove)
                return;

            Think();

            if (!Alive || MarkedToRemove)
                return;

            CurrentState?.OnFrame();

            if (!Alive || MarkedToRemove)
                return;
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
            wasOffScreen = false;
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
            wasOffScreen = false;
            Engine.spawnedEntities.Add(this);
        }

        protected internal virtual void OnSpawn()
        {
            SpawnEvent?.Invoke(this);
        }

        protected internal virtual void PostSpawn()
        {
            Spawning = false;
            Alive = true;
            Dead = false;

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

        protected virtual void OnOffScreen()
        {            
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

        public virtual bool IsOffscreen(VectorKind kind, bool extendedCamera = true)
        {
            return GetVector(kind) > (extendedCamera ? Engine.World.Camera.ExtendedBoundingBox : Engine.World.Camera.BoundingBox);
        }

        public virtual bool IsOffscreen(BoxKind kind, bool extendedCamera = true)
        {
            return !CollisionChecker.HasIntersection(GetBox(kind), extendedCamera ? Engine.World.Camera.ExtendedBoundingBox : Engine.World.Camera.BoundingBox);
        }

        public virtual void Place()
        {
            Respawnable = true;
            RespawnOnNear = true;

            if (!Engine.autoRespawnableEntities.ContainsKey(this))
                Engine.autoRespawnableEntities.Add(this, new RespawnEntry(this, Origin));

            UpdatePartition(true);
        }

        public virtual void Unplace()
        {
            Respawnable = false;
            RespawnOnNear = false;

            if (Engine.autoRespawnableEntities.ContainsKey(this))
                Engine.autoRespawnableEntities.Remove(this);

            UpdatePartition(true);
        }

        public Direction GetHorizontalDirection(Vector pos)
        {
            return Origin.X < pos.X ? Direction.LEFT : Direction.RIGHT;
        }

        public Direction GetVerticalDirection(Vector pos)
        {
            return Origin.Y < pos.Y ? Direction.UP : Direction.DOWN;
        }

        public Direction GetHorizontalDirection(Entity entity)
        {
            return GetHorizontalDirection(entity.Origin);
        }

        public Direction GetVerticalDirection(Entity entity)
        {
            return GetVerticalDirection(entity.Origin);
        }
    }
}