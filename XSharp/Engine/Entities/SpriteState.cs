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

        protected internal override void OnStart()
        {
            base.OnStart();

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