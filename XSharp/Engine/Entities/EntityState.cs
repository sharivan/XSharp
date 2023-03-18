using System;

using SerializableAttribute = XSharp.Serialization.SerializableAttribute;

namespace XSharp.Engine.Entities;

public delegate void EntityStateStartEvent(EntityState state, EntityState lastState);
public delegate void EntityStateFrameEvent(EntityState state, long frameCounter);
public delegate void EntityStateEndEvent(EntityState state);

[Serializable]
public class EntityState
{
    public event EntityStateStartEvent StartEvent;
    public event EntityStateFrameEvent FrameEvent;
    public event EntityStateEndEvent EndEvent;

    internal EntityReference entity;

    private EntitySubState[] subStates;
    private int currentSubStateID = -1;

    public Entity Entity => entity;

    public bool Current => Entity.CurrentStateID == ID;

    public int ID
    {
        get;
        internal set;
    } = -1;

    public long FrameCounter
    {
        get;
        private set;
    } = 0;

    public bool HasSubStates => subStates != null && subStates.Length > 0;

    public int SubStateCount => subStates == null ? 0 : subStates.Length;

    public int InitialSubStateID
    {
        get;
        set;
    } = 0;

    public int CurrentSubStateID
    {
        get => currentSubStateID;
        set
        {
            if (currentSubStateID != value)
            {
                var lastSubState = CurrentSubState;
                lastSubState?.OnEnd();
                currentSubStateID = value;
                CurrentSubState?.OnStart(null, lastSubState);
            }
        }
    }

    public EntitySubState CurrentSubState => currentSubStateID >= 0 ? subStates[currentSubStateID] : null;

    internal void InitializeSubStates(int subStateCount)
    {
        subStates = new EntitySubState[subStateCount];
    }

    protected virtual Type GetSubStateType()
    {
        return typeof(EntitySubState);
    }

    public EntitySubState RegisterSubState(int id, EntitySubStateStartEvent onStart, EntitySubStateFrameEvent onFrame, EntitySubStateEndEvent onEnd)
    {
        var subState = (EntitySubState) Activator.CreateInstance(GetSubStateType());
        subState.entity = entity;
        subState.stateID = ID;
        subState.ID = id;
        subState.StartEvent += onStart;
        subState.FrameEvent += onFrame;
        subState.EndEvent += onEnd;
        subStates[id] = subState;
        return subState;
    }

    public EntitySubState RegisterSubState(int id, EntitySubStateStartEvent onStart)
    {
        return RegisterSubState(id, onStart, null, null);
    }

    public EntitySubState RegisterSubState(int id, EntitySubStateFrameEvent onFrame)
    {
        return RegisterSubState(id, null, onFrame, null);
    }

    public EntitySubState RegisterSubState(int id, EntitySubStateEndEvent onEnd)
    {
        return RegisterSubState(id, null, null, onEnd);
    }

    public EntitySubState RegisterSubState(int id)
    {
        return RegisterSubState(id, null, null, null);
    }

    protected internal virtual void OnStart(EntityState lastState)
    {
        FrameCounter = 0;
        StartEvent?.Invoke(this, lastState);

        if (Current && HasSubStates)
        {
            var lastSubState = lastState != null && lastState.HasSubStates ? lastState.CurrentSubState : null;
            lastSubState?.OnEnd();

            switch (Entity.SubStateChangeMode)
            {
                case SubStateChangeMode.PRESERVE_LAST:
                    currentSubStateID = lastSubState != null ? lastSubState.ID : InitialSubStateID;
                    break;

                case SubStateChangeMode.PRESERVE_CURRENT:
                    if (currentSubStateID == -1)
                        currentSubStateID = InitialSubStateID;

                    break;

                case SubStateChangeMode.RESET_CURRENT:
                    currentSubStateID = InitialSubStateID;
                    break;
            }
            
            CurrentSubState?.OnStart(lastState, lastSubState);
        }
    }

    protected internal virtual void OnFrame()
    {
        FrameEvent?.Invoke(this, FrameCounter);

        if (HasSubStates)
            CurrentSubState?.OnFrame();

        FrameCounter++;
    }

    protected internal virtual void OnEnd()
    {
        EndEvent?.Invoke(this);

        if (HasSubStates)
            CurrentSubState?.OnEnd();
    }
}

public delegate void EntitySubStateStartEvent(EntityState state, EntityState lastState, EntitySubState subState, EntitySubState lastSubState);
public delegate void EntitySubStateFrameEvent(EntityState state, EntitySubState subState, long frameCounter);
public delegate void EntitySubStateEndEvent(EntityState state, EntitySubState subState);

[Serializable]
public class EntitySubState
{
    public event EntitySubStateStartEvent StartEvent;
    public event EntitySubStateFrameEvent FrameEvent;
    public event EntitySubStateEndEvent EndEvent;

    internal EntityReference entity = null;
    internal int stateID = -1;

    public Entity Entity => entity;

    public bool Current
    {
        get
        {
            if (Entity.CurrentStateID != stateID)
                return false;

            var currentState = Entity.GetStateByID(Entity.CurrentStateID);
            return currentState != null && currentState.HasSubStates && currentState.CurrentSubState == this;
        }
    }

    public EntityState State => stateID != -1 ? Entity.GetStateByID(stateID) : null;

    public int ID
    {
        get;
        internal set;
    } = -1;

    public long FrameCounter
    {
        get;
        private set;
    } = 0;

    protected internal virtual void OnStart(EntityState lastState, EntitySubState lastSubState)
    {
        FrameCounter = 0;
        StartEvent?.Invoke(State, lastState, this, lastSubState);
    }

    protected internal virtual void OnFrame()
    {
        FrameEvent?.Invoke(State, this, FrameCounter);
        FrameCounter++;
    }

    protected internal virtual void OnEnd()
    {
        EndEvent?.Invoke(State, this);
    }
}