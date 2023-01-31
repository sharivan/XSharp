using MMX.Geometry;
using MMX.Math;

namespace MMX.Engine.Entities.Effects
{
    public class XDieExplosion : SpriteEffect
    {
        private int frameCounter;

        public int MaxFrames
        {
            get;
            set;
        }

        public int FramesPerCicle
        {
            get;
            set;
        }

        public double MaxRadius
        {
            get;
            set;
        }

        public double Phase
        {
            get;
            set;
        }

        public int SparkCount
        {
            get;
            set;
        }

        public XDieExplosion(GameEngine engine, string name, Vector origin, int maxFrames, int framesPerCicle, double maxRadius, double phase, int sparkCount)
            : base(engine, name, origin, 0, false, "DyingExplosion")
        {
            MaxFrames = maxFrames;
            FramesPerCicle = framesPerCicle;
            MaxRadius = maxRadius;            
            Phase = phase;
            SparkCount = sparkCount;
        }

        public XDieExplosion(GameEngine engine, string name, Vector origin, double phase) :
            this(engine, name, origin, 68, 128, 140, phase, 8)
        {
        }

        protected internal override void OnSpawn()
        {
            base.OnSpawn();

            MultiAnimation = true;
            PaletteIndex = 0;
            frameCounter = 0;
        }

        protected override void Think()
        {
            base.Think();

            double radius = (double) frameCounter / MaxFrames * MaxRadius;
            for (int i = 0; i < SparkCount; i++)
            {
                Animation animation = GetAnimation(i);
                double angle = ((double) frameCounter / FramesPerCicle + (double) i / SparkCount) * 2 * System.Math.PI + Phase;
                animation.Offset = (radius * System.Math.Cos(angle), radius * System.Math.Sin(angle));
            }

            frameCounter++;

            if (frameCounter == MaxFrames)
                Kill();
        }

        protected override void OnCreateAnimation(int animationIndex, SpriteSheet sheet, string frameSequenceName, ref Vector offset, ref int count, ref int repeatX, ref int repeatY, ref int initialFrame, ref bool startVisible, ref bool startOn, ref bool add)
        {
            base.OnCreateAnimation(animationIndex, sheet, frameSequenceName, ref offset, ref count, ref repeatX, ref repeatY, ref initialFrame, ref startVisible, ref startOn, ref add);

            switch (frameSequenceName)
            {
                case "DyingExplosion":
                    count = SparkCount;
                    startOn = true;
                    startVisible = true;
                    break;

                default:
                    add = false;
                    break;
            }
        }
    }
}