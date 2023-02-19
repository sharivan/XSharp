using XSharp.Engine.Entities;
using XSharp.Geometry;
using XSharp.Math;
using static XSharp.Engine.Consts;

namespace XSharp.Engine.World
{
    public class WorldCollider : Collider
    {
        private FixedSingle headHeight;
        private FixedSingle legsHeight;

        private CollisionChecker headCollisionChecker;
        private CollisionChecker chestCollisionChecker;
        private CollisionChecker legsCollisionChecker;

        public Box HeadBox
        {
            get;
            private set;
        }

        public Box ChestBox
        {
            get;
            private set;
        }

        public Box LegsBox
        {
            get;
            private set;
        }

        public FixedSingle HeadHeight
        {
            get => headHeight;

            set
            {
                headHeight = value;
                UpdateColliders();
            }
        }

        public FixedSingle LegsHeight
        {
            get => legsHeight;

            set
            {
                legsHeight = value;
                UpdateColliders();
            }
        }

        public WorldCollider(Sprite owner, Box box, bool useCollisionPlacements = false)
            : this(owner, box, STEP_SIZE, 0, 0, useCollisionPlacements)
        {
        }

        public WorldCollider(Sprite owner, Box box, FixedSingle stepSize, FixedSingle headHeight, FixedSingle legsHeight, bool useCollisionPlacements = false, bool checkCollisionWithWorld = true, bool checkCollisionWithSolidSprites = false)
            : base(owner, box, stepSize, useCollisionPlacements, checkCollisionWithWorld, checkCollisionWithSolidSprites)
        {
            this.headHeight = headHeight;
            this.legsHeight = legsHeight;

            headCollisionChecker = new CollisionChecker()
            {
                ComputePlacements = useCollisionPlacements,
                StepSize = stepSize,
                CheckWithWorld = checkCollisionWithWorld,
                CheckWithSolidSprites = checkCollisionWithSolidSprites
            };

            chestCollisionChecker = new CollisionChecker()
            {
                ComputePlacements = useCollisionPlacements,
                StepSize = stepSize,
                CheckWithWorld = checkCollisionWithWorld,
                CheckWithSolidSprites = checkCollisionWithSolidSprites
            };

            legsCollisionChecker = new CollisionChecker()
            {
                ComputePlacements = useCollisionPlacements,
                StepSize = stepSize,
                CheckWithWorld = checkCollisionWithWorld,
                CheckWithSolidSprites = checkCollisionWithSolidSprites
            };
        }

        protected override void UpdateColliders()
        {
            HeadBox = new Box(Box.LeftTop, Box.Width, headHeight);
            ChestBox = new Box(Box.Left, Box.Top + headHeight, Box.Width, Box.Height - headHeight - legsHeight);
            LegsBox = new Box(Box.Left, Box.Bottom - legsHeight, Box.Width, legsHeight);

            headCollisionChecker.Setup(HeadBox, CollisionFlags.NONE, IgnoreSprites, StepSize, CheckCollisionWithWorld, CheckCollisionWithSolidSprites, UseCollisionPlacements, true);
            chestCollisionChecker.Setup(ChestBox, CollisionFlags.NONE, IgnoreSprites, StepSize, CheckCollisionWithWorld, CheckCollisionWithSolidSprites, UseCollisionPlacements, true);
            legsCollisionChecker.Setup(LegsBox, CollisionFlags.NONE, IgnoreSprites, StepSize, CheckCollisionWithWorld, CheckCollisionWithSolidSprites, UseCollisionPlacements, true);

            base.UpdateColliders();
        }

        protected override Box GetMoveLeftBox()
        {
            return ChestBox;
        }

        protected override Box GetMoveUpBox()
        {
            return HeadBox;
        }

        protected override Box GetMoveRightBox()
        {
            return ChestBox;
        }

        protected override Box GetMoveDownBox()
        {
            return LegsBox;
        }

        protected override CollisionChecker GetMoveLeftCollisionChecker()
        {
            return chestCollisionChecker;
        }

        protected override CollisionChecker GetMoveUpCollisionChecker()
        {
            return headCollisionChecker;
        }

        protected override CollisionChecker GetMoveRightCollisionChecker()
        {
            return chestCollisionChecker;
        }

        protected override CollisionChecker GetMoveDownCollisionChecker()
        {
            return legsCollisionChecker;
        }
    }
}