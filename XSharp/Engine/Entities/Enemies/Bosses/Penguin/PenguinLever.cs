namespace XSharp.Engine.Entities.Enemies.Bosses.Penguin
{
    public class PenguinLever : Sprite
    {
        public PenguinLever()
        {
            Directional = true;
            SpriteSheetIndex = 10;
            PaletteIndex = 7;

            SetAnimationNames("Lever");
        }
    }
}