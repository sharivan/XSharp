using System;

using SerializableAttribute = XSharp.Serialization.SerializableAttribute;

namespace XSharp.Engine.Entities;

[Serializable]
public class SpriteState : EntityState
{
    private string animationName;

    public Sprite Sprite => (Sprite) Entity;

    public int AnimationIndex
    {
        get;
        set;
    } = -1;

    public int InitialFrame
    {
        get;
        set;
    } = 0;

    public string AnimationName
    {
        get => animationName;
        set
        {
            animationName = value;
            AnimationIndex = Sprite != null ? Sprite.GetAnimationIndexByName(value) : -1;
        }
    }

    new public SpriteSubState CurrentSubState => (SpriteSubState) base.CurrentSubState;

    protected override Type GetSubStateType()
    {
        return typeof(SpriteSubState);
    }

    public SpriteSubState RegisterSubState(int id, EntitySubStateStartEvent onStart, EntitySubStateFrameEvent onFrame, EntitySubStateEndEvent onEnd, string animationName, int initialFrame = 0)
    {
        var subState = (SpriteSubState) RegisterSubState(id, onStart, onFrame, onEnd);
        subState.AnimationName = animationName;
        subState.InitialFrame = initialFrame;
        return subState;
    }

    public SpriteSubState RegisterSubState(int id, EntitySubStateStartEvent onStart, string animationName, int initialFrame = 0)
    {
        return RegisterSubState(id, onStart, null, null, animationName, initialFrame);
    }

    public SpriteSubState RegisterSubState(int id, EntitySubStateFrameEvent onFrame, string animationName, int initialFrame = 0)
    {
        return RegisterSubState(id, null, onFrame, null, animationName, initialFrame);
    }

    public SpriteSubState RegisterSubState(int id, EntitySubStateEndEvent onEnd, string animationName, int initialFrame = 0)
    {
        return RegisterSubState(id, null, null, onEnd, animationName, initialFrame);
    }

    public SpriteSubState RegisterSubState(int id, string animationName, int initialFrame = 0)
    {
        return RegisterSubState(id, null, null, null, animationName, initialFrame);
    }

    public SpriteSubState RegisterSubState<T>(T id, EntitySubStateStartEvent onStart, EntitySubStateFrameEvent onFrame, EntitySubStateEndEvent onEnd, string animationName, int initialFrame = 0) where T : struct, Enum
    {
        return RegisterSubState((int) (object) id, onStart, onFrame, onEnd, animationName, initialFrame);
    }

    public SpriteSubState RegisterSubState<T>(T id, EntitySubStateStartEvent onStart, string animationName, int initialFrame = 0) where T : struct, Enum
    {
        return RegisterSubState((int) (object) id, onStart, null, null, animationName, initialFrame);
    }

    public SpriteSubState RegisterSubState<T>(T id, EntitySubStateFrameEvent onFrame, string animationName, int initialFrame = 0) where T : struct, Enum
    {
        return RegisterSubState((int) (object) id, null, onFrame, null, animationName, initialFrame);
    }

    public SpriteSubState RegisterSubState<T>(T id, EntitySubStateEndEvent onEnd, string animationName, int initialFrame = 0) where T : struct, Enum
    {
        return RegisterSubState((int) (object) id, null, null, onEnd, animationName, initialFrame);
    }

    public SpriteSubState RegisterSubState<T>(T id, string animationName, int initialFrame = 0) where T : struct, Enum
    {
        return RegisterSubState((int) (object) id, null, null, null, animationName, initialFrame);
    }

    public SpriteSubState RegisterSubState(int id, EntitySubStateStartEvent onStart, EntitySubStateFrameEvent onFrame, EntitySubStateEndEvent onEnd, int animationIndex, int initialFrame = 0)
    {
        var subState = (SpriteSubState) RegisterSubState(id, onStart, onFrame, onEnd);
        subState.AnimationIndex = animationIndex;
        subState.InitialFrame = initialFrame;
        return subState;
    }

    public SpriteSubState RegisterSubState(int id, EntitySubStateStartEvent onStart, int animationIndex, int initialFrame = 0)
    {
        return RegisterSubState(id, onStart, null, null, animationIndex, initialFrame);
    }

    public SpriteSubState RegisterSubState(int id, EntitySubStateFrameEvent onFrame, int animationIndex, int initialFrame = 0)
    {
        return RegisterSubState(id, null, onFrame, null, animationIndex, initialFrame);
    }

    public SpriteSubState RegisterSubState(int id, EntitySubStateEndEvent onEnd, int animationIndex, int initialFrame = 0)
    {
        return RegisterSubState(id, null, null, onEnd, animationIndex, initialFrame);
    }

    public SpriteSubState RegisterSubState(int id, int animationIndex, int initialFrame = 0)
    {
        return RegisterSubState(id, null, null, null, animationIndex, initialFrame);
    }

    public SpriteSubState RegisterSubState<T>(T id, EntitySubStateStartEvent onStart, EntitySubStateFrameEvent onFrame, EntitySubStateEndEvent onEnd, int animationIndex, int initialFrame = 0) where T : struct, Enum
    {
        return RegisterSubState((int) (object) id, onStart, onFrame, onEnd, animationIndex, initialFrame);
    }

    public SpriteSubState RegisterSubState<T>(T id, EntitySubStateStartEvent onStart, int animationIndex, int initialFrame = 0) where T : struct, Enum
    {
        return RegisterSubState((int) (object) id, onStart, null, null, animationIndex, initialFrame);
    }

    public SpriteSubState RegisterSubState<T>(T id, EntitySubStateFrameEvent onFrame, int animationIndex, int initialFrame = 0) where T : struct, Enum
    {
        return RegisterSubState((int) (object) id, null, onFrame, null, animationIndex, initialFrame);
    }

    public SpriteSubState RegisterSubState<T>(T id, EntitySubStateEndEvent onEnd, int animationIndex, int initialFrame = 0) where T : struct, Enum
    {
        return RegisterSubState((int) (object) id, null, null, onEnd, animationIndex, initialFrame);
    }

    public SpriteSubState RegisterSubState<T>(T id, int animationIndex, int initialFrame = 0) where T : struct, Enum
    {
        return RegisterSubState((int) (object) id, null, null, null, animationIndex, initialFrame);
    }

    protected override void OnStart(EntityState lastState)
    {
        base.OnStart(lastState);

        if (Current && Sprite != null && !HasSubStates)
        {
            if (AnimationIndex < 0 && AnimationName != null)
                AnimationIndex = Sprite.GetAnimationIndexByName(AnimationName);

            if (AnimationIndex >= 0)
                Sprite.SetCurrentAnimationByIndex(AnimationIndex, InitialFrame);
        }
    }
}

[Serializable]
public class SpriteSubState : EntitySubState
{
    private string animationName;

    new public SpriteState State => (SpriteState) base.State;

    public Sprite Sprite => State.Sprite;

    public int AnimationIndex
    {
        get;
        set;
    } = -1;

    public int InitialFrame
    {
        get;
        set;
    } = 0;

    public string AnimationName
    {
        get => animationName;
        set
        {
            animationName = value;
            AnimationIndex = Sprite != null ? Sprite.GetAnimationIndexByName(value) : -1;
        }
    }

    protected override void OnStart(EntityState lastState, EntitySubState lastSubState)
    {
        base.OnStart(lastState, lastSubState);

        if (Current && Sprite != null)
        {
            if (AnimationIndex < 0 && AnimationName != null)
                AnimationIndex = Sprite.GetAnimationIndexByName(AnimationName);

            if (AnimationIndex >= 0)
                Sprite.SetCurrentAnimationByIndex(AnimationIndex, InitialFrame);
        }
    }
}