using XSharp.Math.Geometry;

namespace XSharp.Engine.Entities.HUD
{
    public class XDieExplosion : HUD
    {
        private int frameCounter;

        public int MaxFrames
        {
            get;
            set;
        } = 68;

        public int FramesPerCicle
        {
            get;
            set;
        } = 128;

        public double MaxRadius
        {
            get;
            set;
        } = 140;

        public double Phase
        {
            get;
            set;
        } = 0;

        public int SparkCount
        {
            get;
            set;
        } = 8;

        public XDieExplosion()
        {
            SpriteSheetIndex = 0;
            PaletteIndex = 0;
            Directional = false;

            SetAnimationNames("DyingExplosion");
        }

        protected internal override void OnSpawn()
        {
            base.OnSpawn();

            MultiAnimation = true;
            frameCounter = 0;
        }

        protected internal override void UpdateOrigin()
        {
            base.UpdateOrigin();

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

        protected override void OnCreateAnimation(int animationIndex, string frameSequenceName, ref Vector offset, ref int count, ref int repeatX, ref int repeatY, ref int initialFrame, ref bool startVisible, ref bool startOn, ref bool add)
        {
            base.OnCreateAnimation(animationIndex, frameSequenceName, ref offset, ref count, ref repeatX, ref repeatY, ref initialFrame, ref startVisible, ref startOn, ref add);

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