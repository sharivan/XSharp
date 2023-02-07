namespace XSharp.Engine.Entities.Enemies.Bosses.Penguin
{
    public class PenguinSnow : Enemy
    {
        public PenguinSnow()
        {
            Directional = true;
            SpriteSheetIndex = 10;
            PaletteIndex = 7;

            SetAnimationNames("Snow");
        }
    }
}