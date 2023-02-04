using MMX.Geometry;
using System.Collections.Generic;

namespace MMX.Engine.Entities.Triggers
{
    public class CameraLockTrigger : AbstractTrigger
    {
        private readonly List<Vector> constraints;

        public IEnumerable<Vector> Constraints => constraints;

        public Vector ConstraintOrigin => BoundingBox.Center;

        public int ConstraintCount => constraints.Count;

        public CameraLockTrigger(GameEngine engine, Box boudingBox)
            : base(engine, boudingBox, TouchingKind.VECTOR)
        {
            constraints = new List<Vector>();
        }

        public CameraLockTrigger(GameEngine engine, Box boudingBox, IEnumerable<Vector> constraints)
            : base(engine, boudingBox, TouchingKind.VECTOR)
        {
            this.constraints = new List<Vector>(constraints);
        }

        protected override void OnStartTrigger(Entity obj)
        {
            base.OnStartTrigger(obj);

            if (obj is Player)
                Engine.SetCameraConstraints(ConstraintOrigin, constraints);
        }

        public void AddConstraint(Vector constraint)
        {
            constraints.Add(constraint);
        }

        public Vector GetConstraint(int index)
        {
            return constraints[index];
        }

        public bool ContainsConstraint(Vector constraint)
        {
            return constraints.Contains(constraint);
        }

        public void ClearConstraints()
        {
            constraints.Clear();
        }
    }
}