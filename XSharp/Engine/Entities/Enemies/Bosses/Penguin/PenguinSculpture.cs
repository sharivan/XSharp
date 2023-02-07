namespace XSharp.Engine.Entities.Enemies.Bosses.Penguin
{
    public class PenguinSculpture : Enemy
    {
        public PenguinSculpture()
        {
            Directional = true;
            SpriteSheetIndex = 10;
            PaletteIndex = 7;

            SetAnimationNames("Sculpture");
        }
    }
}