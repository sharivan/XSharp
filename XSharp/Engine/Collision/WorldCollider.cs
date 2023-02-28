using XSharp.Engine.Entities;
using XSharp.Math;
using XSharp.Math.Geometry;
using static XSharp.Engine.Consts;

namespace XSharp.Engine.Collision
{
    public class WorldCollider : Collider
    {
        private FixedSingle headHeight;
        private FixedSingle legsHeight;

        private TracerCollisionChecker headCollisionChecker;
        private TracerCollisionChecker chestCollisionChecker;
        private TracerCollisionChecker legsCollisionChecker;

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
            : this(owner, box, 0, 0, useCollisionPlacements)
        {
        }

        public WorldCollider(Sprite owner, Box box, FixedSingle headHeight, FixedSingle legsHeight, bool useCollisionPlacements = false, bool checkCollisionWithWorld = true, bool checkCollisionWithSolidSprites = false)
            : base(owner, box, useCollisionPlacements, checkCollisionWithWorld, checkCollisionWithSolidSprites)
        {
            this.headHeight = headHeight;
            this.legsHeight = legsHeight;

            headCollisionChecker = new TracerCollisionChecker()
            {
                ComputePlacements = useCollisionPlacements,
                CheckWithWorld = checkCollisionWithWorld,
                CheckWithSolidSprites = checkCollisionWithSolidSprites
            };

            chestCollisionChecker = new TracerCollisionChecker()
            {
                ComputePlacements = useCollisionPlacements,
                CheckWithWorld = checkCollisionWithWorld,
                CheckWithSolidSprites = checkCollisionWithSolidSprites
            };

            legsCollisionChecker = new TracerCollisionChecker()
            {
                ComputePlacements = useCollisionPlacements,
                CheckWithWorld = checkCollisionWithWorld,
                CheckWithSolidSprites = checkCollisionWithSolidSprites
            };
        }

        protected override void UpdateColliders()
        {
            HeadBox = new Box(Box.LeftTop, Box.Width, headHeight);
            ChestBox = new Box(Box.Left, Box.Top + headHeight, Box.Width, Box.Height - headHeight - legsHeight);
            LegsBox = new Box(Box.Left, Box.Bottom - legsHeight, Box.Width, legsHeight);

            headCollisionChecker.Setup(HeadBox, CollisionFlags.NONE, IgnoreSprites, CheckCollisionWithWorld, CheckCollisionWithSolidSprites, UseCollisionPlacements);
            chestCollisionChecker.Setup(ChestBox, CollisionFlags.NONE, IgnoreSprites, CheckCollisionWithWorld, CheckCollisionWithSolidSprites, UseCollisionPlacements);
            legsCollisionChecker.Setup(LegsBox, CollisionFlags.NONE, IgnoreSprites, CheckCollisionWithWorld, CheckCollisionWithSolidSprites, UseCollisionPlacements);

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

        protected override TracerCollisionChecker GetMoveLeftCollisionChecker()
        {
            return chestCollisionChecker;
        }

        protected override TracerCollisionChecker GetMoveUpCollisionChecker()
        {
            return headCollisionChecker;
        }

        protected override TracerCollisionChecker GetMoveRightCollisionChecker()
        {
            return chestCollisionChecker;
        }

        protected override TracerCollisionChecker GetMoveDownCollisionChecker()
        {
            return legsCollisionChecker;
        }
    }
}