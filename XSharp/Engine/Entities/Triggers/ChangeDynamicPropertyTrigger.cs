namespace XSharp.Engine.Entities.Triggers
{
    public enum DynamicProperty
    {
        OBJECT_TILE,
        BACKGROUND_TILE,
        PALETTE
    }

    public class ChangeDynamicPropertyTrigger : SplitterTrigger
    {
        public DynamicProperty Property
        {
            get; set;
        }

        public int Forward
        {
            get; set;
        }

        public int Backward
        {
            get; set;
        }

        public ChangeDynamicPropertyTrigger()
        {
        }

        private void ChangeProperty(int value)
        {
            switch (Property)
            {
                case DynamicProperty.OBJECT_TILE:
                    Engine.ObjectTile = value;
                    break;

                case DynamicProperty.BACKGROUND_TILE:
                    Engine.BackgroundTile = value;
                    break;

                case DynamicProperty.PALETTE:
                    Engine.Palette = value;
                    break;
            }
        }

        protected override void OnSplitterTriggerEvent(Entity obj, SplitterTriggerDirection direction)
        {
            base.OnSplitterTriggerEvent(obj, direction);

            if (obj is not Player)
                return;

            switch (direction)
            {
                case SplitterTriggerDirection.BACKWARD:
                    ChangeProperty(Backward);
                    break;

                case SplitterTriggerDirection.FORWARD:
                    ChangeProperty(Forward);
                    break;
            }
        }
    }
}
