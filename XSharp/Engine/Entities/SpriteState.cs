using System;

namespace XSharp.Engine.Entities
{
    public class SpriteState : EntityState
    {
        private string animationName;

        public Sprite Sprite
        {
            get => (Sprite) Entity;
            set => Entity = value;
        }

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

        protected internal override void OnStart(EntityState lastState)
        {
            base.OnStart(lastState);

            if (Sprite != null && !HasSubStates)
            {
                if (AnimationIndex < 0 && AnimationName != null)
                    AnimationIndex = Sprite.GetAnimationIndexByName(AnimationName);

                if (AnimationIndex >= 0)
                {
                    Sprite.CurrentAnimationIndex = AnimationIndex;
                    Sprite.CurrentAnimation?.Start(InitialFrame);
                }
            }
        }
    }

    public class SpriteSubState : EntitySubState
    {
        private string animationName;

        new public SpriteState State
        {
            get => (SpriteState) base.State;
            set => base.State = value;
        }

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

        protected internal override void OnStart(EntityState lastState, EntitySubState lastSubState)
        {
            base.OnStart(lastState, lastSubState);

            if (Sprite != null)
            {
                if (AnimationIndex < 0 && AnimationName != null)
                    AnimationIndex = Sprite.GetAnimationIndexByName(AnimationName);

                if (AnimationIndex >= 0)
                {
                    Sprite.CurrentAnimationIndex = AnimationIndex;
                    Sprite.CurrentAnimation?.Start(InitialFrame);
                }
            }
        }
    }
}