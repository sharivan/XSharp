using System.Collections.Generic;

using MMX.Math;
using MMX.Geometry;

namespace MMX.Engine.Entities.Effects
{
    public class SpriteEffect : Sprite
    {
        private readonly Dictionary<string, int> animationNames;

        public string InitialAnimationName
        {
            get;
            set;
        }

        public int InitialAnimationIndex => GetAnimationIndex(InitialAnimationName);

        public SpriteEffect(GameEngine engine, string name, Vector origin, int spriteSheetIndex, string[] animationNames, string initialAnimationName, bool directional = false)
            : base(engine, name, origin, spriteSheetIndex, directional)
        {
            InitialAnimationName = initialAnimationName;

            CheckCollisionWithWorld = false;

            this.animationNames = new Dictionary<string, int>();

            foreach (var animationName in animationNames)
                this.animationNames.Add(animationName, -1);
        }

        public SpriteEffect(GameEngine engine, string name, Vector origin, int spriteSheetIndex, bool directional, params string[] animationNames) : this(engine, name, origin, spriteSheetIndex, animationNames, animationNames[0], directional) { }

        public override FixedSingle GetGravity()
        {
            return FixedSingle.ZERO;
        }

        internal override void OnSpawn()
        {
            base.OnSpawn();

            CurrentAnimationIndex = InitialAnimationIndex;
            CurrentAnimation.StartFromBegin();
        }

        protected override void OnCreateAnimation(int animationIndex, SpriteSheet sheet, ref string frameSequenceName, ref int initialFrame, ref bool startVisible, ref bool startOn, ref bool add)
        {
            base.OnCreateAnimation(animationIndex, sheet, ref frameSequenceName, ref initialFrame, ref startVisible, ref startOn, ref add);
            startOn = false;
            startVisible = false;

            if (animationNames.ContainsKey(frameSequenceName))
                animationNames[frameSequenceName] = animationIndex;
            else
                add = false;
        }

        public int GetAnimationIndex(string animationName)
        {
            return animationNames.TryGetValue(animationName, out int result) ? result : -1;
        }
    }
}
