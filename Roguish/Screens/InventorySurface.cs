namespace Roguish.Screens;
internal class InventorySurface : ScreenSurface
{
    public InventorySurface() : base(GameSettings.InvWidth, GameSettings.InvHeight)
    {
        Position = GameSettings.InvPosition;
    }
}
