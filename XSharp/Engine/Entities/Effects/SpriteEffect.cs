using System.Collections.Generic;

using MMX.Math;
using MMX.Geometry;

namespace MMX.Engine.Entities.Effects
{
    public class SpriteEffect : Sprite
    {
        private readonly Dictionary<string, int> animationNames;
        private readonly Dictionary<(string, Direction), int> animationNamesDirection;

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
            animationNamesDirection = new Dictionary<(string, Direction), int>();

            foreach (var animationName in animationNames)
            {
                this.animationNames.Add(animationName, -1);
                animationNamesDirection.Add((animationName, Direction.RIGHT), -1);
                animationNamesDirection.Add((animationName, Direction.LEFT), -1);
            }
        }

        public SpriteEffect(GameEngine engine, string name, Vector origin, int spriteSheetIndex, bool directional, params string[] animationNames) : this(engine, name, origin, spriteSheetIndex, animationNames, animationNames[0], directional) { }

        public override FixedSingle GetGravity() => FixedSingle.ZERO;

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
            {
                animationNames[frameSequenceName] = animationIndex;
                animationNamesDirection[(frameSequenceName, Direction.RIGHT)] = animationIndex;
                animationNamesDirection[(frameSequenceName, Direction.LEFT)] = Directional ? animationIndex + 1 : animationIndex;
            }
            else
                add = false;
        }

        public int GetAnimationIndex(string animationName, Direction direction) => animationNamesDirection.TryGetValue((animationName, direction), out int result) ? result : -1;

        public int GetAnimationIndex(string animationName) => GetAnimationIndex(animationName, Direction);
    }
}
