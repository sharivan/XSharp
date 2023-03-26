using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

using XSharp.Engine.Collision;
using XSharp.Factories;
using XSharp.Math.Geometry;

using static XSharp.Engine.Consts;

using SerializableAttribute = XSharp.Serialization.SerializableAttribute;

namespace XSharp.Engine.Entities;

public enum TouchingKind
{
    VECTOR,
    BOX
}

public delegate void EntityEvent(Entity source);
public delegate void EntityActivatorEvent(Entity source, Entity activator);

[Serializable]
public abstract class Entity : IIndexedNamedFactoryItem
{
    public static GameEngine Engine => GameEngine.Engine;

    public event EntityEvent SpawnEvent;
    public event EntityEvent DeathEvent;
    public event EntityEvent VisibleChangedEvent;
    public event EntityEvent PreThinkEvent;
    public event EntityEvent ThinkEvent;
    public event EntityEvent PostThinkEvent;
    public event EntityActivatorEvent StartTouchEvent;
    public event EntityActivatorEvent TouchingEvent;
    public event EntityActivatorEvent EndTouchEvent;

    private Dictionary<string, object> initParams = new();
    internal int index = -1;
    internal string name = null;
    private Vector origin = Vector.NULL_VECTOR;
    private Direction direction = Direction.RIGHT;
    internal EntityReference parent = null;

    private bool doThink;

    private bool wasOffScreen;
    private bool wasOutOfLiveArea;

    internal EntitySet<Entity> touchingEntities;
    internal EntitySet<Entity> childs;
    private EntitySet<Entity> resultSet;

    internal EntityReference reference;
    internal EntityReference previous;
    internal EntityReference next;

    internal long frameToKill = -1;

    private List<EntityState> states;
    private EntityState[] stateArray;
    private int currentStateID;

    private bool checkTouchingEntities = true;
    private bool checkTouchingWithDeadEntities = false;

    private Box[] lastBox;

    public EntityFactory Factory => Engine.Entities;

    IIndexedNamedFactory IIndexedNamedFactoryItem.Factory => Factory;

    public int Index
    {
        get => index;
        internal set => index = value;
    }

    public string Name
    {
        get => name;
        internal set => Engine.Entities.UpdateEntityName(this, value);
    }

    public Entity Parent
    {
        get => parent;

        set
        {
            if ((Entity) parent == value)
                return;

            if (value != null && value.IsParent(this))
                throw new ArgumentException("Cyclic parenting is not allowed.");

            Parent?.childs.Remove(this);
            parent = value;
            Parent?.childs.Add(this);
        }
    }

    public IEnumerable<Entity> TouchingEntities => touchingEntities;

    public IEnumerable<Entity> Childs => childs;

    public Vector Origin
    {
        get => origin;
        set
        {
            value = value.TruncFracPart();
            LastOrigin = origin;
            Vector delta = value - origin;
            origin = value;

            if (delta != Vector.NULL_VECTOR)
            {
                foreach (Entity child in childs)
                    child.Origin += delta;
            }

            UpdatePartition();
        }
    }

    public Vector IntegerOrigin => Origin.RoundToFloor();

    public Vector LastOrigin
    {
        get;
        internal set;
    }

    public Direction Direction
    {
        get => direction;

        set
        {
            if (value != direction)
            {
                direction = value;

                foreach (Entity child in childs)
                {
                    if (child.UpdateOriginFromParentDirection)
                        child.Origin = child.Origin.Mirror(Origin);

                    if (child.UpdateDirectionFromParent)
                        child.Direction = child.Direction.Oposite();
                }

                UpdatePartition();
            }
        }
    }

    public bool UpdateDirectionFromParent
    {
        get;
        set;
    } = true;

    public bool UpdateOriginFromParentDirection
    {
        get;
        set;
    } = true;

    public Direction DefaultDirection
    {
        get;
        set;
    } = Direction.RIGHT;

    public virtual Box Hitbox
    {
        get
        {
            Box box = Origin + (!Alive && (Respawnable || SpawnOnNear) ? GetDeadBox() : GetHitbox());
            return Direction != DefaultDirection ? box.Mirror(Origin) : box;
        }

        protected set
        {
            Box hitbox = Direction != DefaultDirection ? value.Mirror(Origin) : value;
            hitbox -= Origin;
            SetHitbox(hitbox);
        }
    }

    public virtual Box TouchingBox
    {
        get
        {
            Box box = Origin + GetTouchingBox();
            return Direction != DefaultDirection ? box.Mirror(Origin) : box;
        }
    }

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

    public bool SpawnOnNear
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
            if (value && !checkTouchingEntities)
                Engine.partition.Insert(this);
            else if (!value && checkTouchingEntities)
                Engine.partition.Remove(this);

            checkTouchingEntities = value;

            UpdateLastBoxes();
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

    public SubStateChangeMode SubStateChangeMode
    {
        get;
        set;
    } = SubStateChangeMode.PRESERVE_LAST;

    public int CurrentStateID
    {
        get => currentStateID;
        set => SetCurrentStateID(value);
    }

    protected EntityState CurrentState
    {
        get => stateArray != null && CurrentStateID >= 0 ? stateArray[CurrentStateID] : null;
        set => CurrentStateID = value != null ? value.ID : -1;
    }

    public bool KillOnOffscreen
    {
        get;
        set;
    } = false;

    public long FrameCounter
    {
        get;
        private set;
    }

    protected Entity()
    {
        touchingEntities = new EntitySet<Entity>();
        childs = new EntitySet<Entity>();
        resultSet = new EntitySet<Entity>();
        states = new List<EntityState>();
        lastBox = new Box[BOXKIND_COUNT];
    }

    internal void Initialize(dynamic initParams)
    {
        if (initParams == null)
            return;

        Type type = GetType();
        Type attrsType = initParams.GetType();
        PropertyInfo[] attributes = attrsType.GetProperties(BindingFlags.Instance | BindingFlags.Public);
        foreach (var attr in attributes)
        {
            string attrName = attr.Name;
            object value = attr.GetValue(initParams);

            var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(prop => prop.Name == attrName);
            var property = (properties.FirstOrDefault(prop => prop.DeclaringType == type) ?? properties.First()) ?? throw new ArgumentException($"Attribute '{attr}' in entity class '{type.Name}' doesn't exist or isn't public.");

            if (!property.CanWrite)
                throw new ArgumentException($"Field or property '{attr}' is not writable.");

            if (property.GetCustomAttribute(typeof(NotStartupableAttribute)) != null)
                throw new ArgumentException($"Field or property '{attr}' can't be initialized by this way.");

            Type propertyType = property.PropertyType;
            Type valueType = value.GetType();
            if (valueType != propertyType && !propertyType.IsAssignableFrom(valueType))
            {
                TypeConverter conv = TypeDescriptor.GetConverter(propertyType);
                value = conv.ConvertFrom(value);
            }

            this.initParams[property.Name] = value;
        }
    }

    protected internal void ResetFromInitParams()
    {
        Type type = GetType();
        foreach (var kv in initParams)
        {
            string name = kv.Key;
            object value = kv.Value;

            var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(prop => prop.Name == name);
            var property = properties.FirstOrDefault(prop => prop.DeclaringType == type) ?? properties.First();

            Type propertyType = property.PropertyType;
            Type valueType = value.GetType();
            if (valueType != propertyType && !propertyType.IsAssignableFrom(valueType))
            {
                TypeConverter conv = TypeDescriptor.GetConverter(propertyType);
                value = conv.ConvertFrom(value);
            }

            property.SetValue(this, value);
        }
    }

    internal void NotifyCreated()
    {
        OnCreate();
    }

    protected virtual void OnCreate()
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

    protected void SetupStateArray<T>() where T : struct, Enum
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

        state.entity = this;
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

    protected EntityState RegisterState<T>(T id, EntityStateStartEvent onStart, EntityStateFrameEvent onFrame, EntityStateEndEvent onEnd) where T : struct, Enum
    {
        return RegisterState((int) (object) id, onStart, onFrame, onEnd, 0);
    }

    protected EntityState RegisterState<T>(T id, EntityStateStartEvent onStart) where T : struct, Enum
    {
        return RegisterState((int) (object) id, onStart, null, null, 0);
    }

    protected EntityState RegisterState<T>(T id, EntityStateFrameEvent onFrame) where T : struct, Enum
    {
        return RegisterState((int) (object) id, null, onFrame, null, 0);
    }

    protected EntityState RegisterState<T>(T id, EntityStateEndEvent onEnd) where T : struct, Enum
    {
        return RegisterState((int) (object) id, null, null, onEnd, 0);
    }

    protected EntityState RegisterState<T>(T id) where T : struct, Enum
    {
        return RegisterState((int) (object) id, null, null, null, 0);
    }

    protected EntityState RegisterState<T, U>(T id, EntityStateStartEvent onStart, EntityStateFrameEvent onFrame, EntityStateEndEvent onEnd) where T : struct, Enum where U : struct, Enum
    {
        return RegisterState((int) (object) id, onStart, onFrame, onEnd, Enum.GetNames(typeof(U)).Length);
    }

    protected EntityState RegisterState<T, U>(T id, EntityStateStartEvent onStart) where T : struct, Enum where U : struct, Enum
    {
        return RegisterState((int) (object) id, onStart, null, null, Enum.GetNames(typeof(U)).Length);
    }

    protected EntityState RegisterState<T, U>(T id, EntityStateFrameEvent onFrame) where T : struct, Enum where U : struct, Enum
    {
        return RegisterState((int) (object) id, null, onFrame, null, Enum.GetNames(typeof(U)).Length);
    }

    protected EntityState RegisterState<T, U>(T id, EntityStateEndEvent onEnd) where T : struct, Enum where U : struct, Enum
    {
        return RegisterState((int) (object) id, null, null, onEnd, Enum.GetNames(typeof(U)).Length);
    }

    protected EntityState RegisterState<T, U>(T id) where T : struct, Enum where U : struct, Enum
    {
        return RegisterState((int) (object) id, null, null, null, Enum.GetNames(typeof(U)).Length);
    }

    protected void UnregisterState(int id)
    {
        EntityState state = stateArray[id];
        stateArray[id] = null;
        states.Remove(state);
    }

    protected internal EntityState GetStateByID(int id)
    {
        return stateArray[id];
    }

    protected internal T GetState<T>() where T : struct, Enum
    {
        return (T) (object) CurrentStateID;
    }

    protected internal T GetSubState<T>() where T : struct, Enum
    {
        return (T) (object) GetStateByID(CurrentStateID).CurrentSubStateID;
    }

    protected internal (U id, V subid) GetState<U, V>()
        where U : struct, Enum
        where V : struct, Enum
    {
        U id = GetState<U>();
        V subid = GetSubState<V>();
        return (id, subid);
    }

    protected internal void SetCurrentStateID(int id, bool resetFrameCounter = true)
    {
        if (currentStateID != id)
        {
            EntityState lastState = CurrentState;
            lastState?.NotifyEnd();

            currentStateID = id;

            EntityState currentState = CurrentState;
            currentState?.NotifyStart(lastState, resetFrameCounter);
        }
    }

    protected internal void SetState<T>(T id, bool resetFrameCounter = true) where T : struct, Enum
    {
        SetCurrentStateID((int) (object) id, resetFrameCounter);
    }

    protected internal void SetSubState<T>(T id, bool resetFrameCounter = true) where T : struct, Enum
    {
        GetStateByID(CurrentStateID).SetCurrentSubStateID((int) (object) id, resetFrameCounter);
    }

    protected internal void SetState<U, V>(U id, V subid)
        where U : struct, Enum
        where V : struct, Enum
    {
        SetState(id);
        SetSubState(subid);
    }

    public bool IsTouching(Entity other)
    {
        return touchingEntities.Contains(other);
    }

    public bool Contains(Entity other)
    {
        return childs.Contains(other);
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

    public override int GetHashCode()
    {
        return index;
    }

    public override string ToString()
    {
        return $"{GetType().Name}[{Name}, {Origin}]";
    }

    protected virtual bool CheckTouching(Entity entity)
    {
        if (TouchingKind == TouchingKind.VECTOR)
        {
            Vector v = entity.GetVector(TouchingVectorKind).RoundToFloor();
            return Hitbox.RoundOriginToFloor().Contains(v);
        }

        return Hitbox.RoundOriginToFloor().IsOverlaping(entity.Hitbox.RoundOriginToFloor());
    }

    internal void DoFrame()
    {
        OnFrame();
    }

    protected virtual void OnFrame()
    {
        if (!Alive || MarkedToRemove)
            return;

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

        if (!Alive || MarkedToRemove)
            return;

        bool outOfLiveArea = !IsInLiveArea(VectorKind.ORIGIN);
        if (outOfLiveArea && !wasOutOfLiveArea)
            OnOutOfLiveArea();

        wasOutOfLiveArea = outOfLiveArea;

        if (!Alive || MarkedToRemove)
            return;

        if (KillOnOffscreen && FrameCounter >= MinimumIntervalToKillOnOffScreen && outOfLiveArea)
        {
            Kill();
            return;
        }

        if (CheckTouchingEntities)
        {
            resultSet.Clear();
            Engine.partition.Query(resultSet, TouchingBox, this, childs, !CheckTouchingWithDeadEntities);

            foreach (var entity in touchingEntities)
            {
                bool touching = CheckTouching(entity);

                if (!touching || !resultSet.Contains(entity))
                {
                    touchingEntities.Remove(entity);
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
            {
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
        }

        if (!Alive || MarkedToRemove)
            return;

        if (doThink)
            OnThink();

        if (!Alive || MarkedToRemove)
            return;

        CurrentState?.DoFrame();

        FrameCounter++;
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

    internal void PreThink()
    {
        doThink = Alive && !MarkedToRemove && OnPreThink();
    }

    protected virtual bool OnPreThink()
    {
        PreThinkEvent?.Invoke(this);
        return true;
    }

    protected virtual void OnThink()
    {
        ThinkEvent?.Invoke(this);
    }

    internal void PostThink()
    {
        OnPostThink();
    }

    protected virtual void OnPostThink()
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

    internal void Cleanup()
    {
        OnCleanup();
    }

    protected virtual void OnCleanup()
    {
        foreach (Entity child in childs)
            child.Parent = null;

        childs.Clear();
        touchingEntities.Clear();

        Parent = null;
        frameToKill = -1;
        currentStateID = -1;
        MarkedToRemove = false;
        Spawning = false;
        wasOffScreen = false;
        wasOutOfLiveArea = false;
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
        Direction = DefaultDirection;
        FrameCounter = 0;
        SpawnFrame = Engine.FrameCounter;
        Spawning = true;
        CheckTouchingEntities = true;
        frameToKill = -1;
        currentStateID = -1;
        MarkedToRemove = false;
        wasOffScreen = false;
        wasOutOfLiveArea = false;
        Engine.spawnedEntities.Add(this);
    }

    internal void NotifySpawn()
    {
        OnSpawn();
    }

    protected virtual void OnSpawn()
    {
        LastOrigin = Origin;

        SpawnEvent?.Invoke(this);
    }

    internal void PostSpawn()
    {
        OnPostSpawn();
    }

    protected virtual void OnPostSpawn()
    {
        Spawning = false;
        Alive = true;
        Dead = false;

        ResetFromInitParams();

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

    protected virtual void OnOutOfLiveArea()
    {
    }

    public void BeginUpdate()
    {
        Updating = true;
    }

    public void EndUpdate()
    {
        Updating = false;
    }

    protected internal void UpdatePartition(bool force = false)
    {
        if (Updating || Index <= 0)
            return;

        if (CheckTouchingEntities)
            Engine.partition.Update(this, force);

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

    public virtual bool IsOffscreen(VectorKind kind)
    {
        return !Engine.Camera.BoundingBox.RoundOriginToFloor().Contains(GetVector(kind).RoundToFloor());
    }

    public virtual bool IsOffscreen(BoxKind kind)
    {
        return !CollisionChecker.HasIntersection(GetBox(kind).RoundOriginToFloor(), Engine.Camera.BoundingBox.RoundOriginToFloor());
    }

    public virtual bool IsInLiveArea(VectorKind kind)
    {
        return Engine.Camera.LiveBoundingBox.RoundOriginToFloor().Contains(GetVector(kind).RoundToFloor());
    }

    public virtual bool IsInLiveArea(BoxKind kind)
    {
        return CollisionChecker.HasIntersection(GetBox(kind).RoundOriginToFloor(), Engine.Camera.LiveBoundingBox.RoundOriginToFloor());
    }

    public virtual bool IsInSpawnArea(VectorKind kind)
    {
        return Engine.Camera.SpawnBoundingBox.RoundOriginToFloor().Contains(GetVector(kind).RoundToFloor());
    }

    public virtual bool IsInSpawnArea(BoxKind kind)
    {
        return CollisionChecker.HasIntersection(GetBox(kind).RoundOriginToFloor(), Engine.Camera.SpawnBoundingBox.RoundOriginToFloor());
    }

    public virtual void Place(bool respawnable = true)
    {
        Respawnable = respawnable;
        SpawnOnNear = true;

        initParams["Origin"] = Origin;

        UpdatePartition(true);
    }

    public virtual void Unplace()
    {
        Respawnable = false;
        SpawnOnNear = false;

        UpdatePartition(true);
    }

    public Direction GetHorizontalDirection(Vector pos)
    {
        return pos.X < Origin.X ? Direction.LEFT : Direction.RIGHT;
    }

    public Direction GetVerticalDirection(Vector pos)
    {
        return pos.Y < Origin.Y ? Direction.UP : Direction.DOWN;
    }

    public Direction GetHorizontalDirection(Entity entity)
    {
        return GetHorizontalDirection(entity.Origin);
    }

    public Direction GetVerticalDirection(Entity entity)
    {
        return GetVerticalDirection(entity.Origin);
    }

    public static implicit operator EntityReference(Entity entity)
    {
        return entity?.reference;
    }
}