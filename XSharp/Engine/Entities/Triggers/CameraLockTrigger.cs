using System.Collections.Generic;

using MMX.Geometry;

namespace MMX.Engine.Entities.Triggers
{
    public class CameraLockTrigger : AbstractTrigger
    {
        private readonly List<Vector> constraints;

        public IEnumerable<Vector> Constraints => constraints;

        public Vector ConstraintOrigin => BoundingBox.Center;

        public int ConstraintCount => constraints.Count;

        public CameraLockTrigger(GameEngine engine, Box boudingBox) :
            base(engine, boudingBox, TouchingKind.VECTOR, VectorKind.PLAYER_ORIGIN) => constraints = new List<Vector>();

        public CameraLockTrigger(GameEngine engine, Box boudingBox, IEnumerable<Vector> constraints) :
            base(engine, boudingBox, TouchingKind.VECTOR, VectorKind.PLAYER_ORIGIN) => this.constraints = new List<Vector>(constraints);

        protected override void OnTrigger(Entity obj)
        {
            base.OnTrigger(obj);

            if (obj is not Player)
                return;

            Engine.SetCameraConstraints(ConstraintOrigin, constraints);
        }

        public void AddConstraint(Vector constraint) => constraints.Add(constraint);

        public Vector GetConstraint(int index) => constraints[index];

        public bool ContainsConstraint(Vector constraint) => constraints.Contains(constraint);

        public void ClearConstraints() => constraints.Clear();
    }
}
