namespace Roguish.Screens;
internal class InfoBar : ScreenSurface
{
    #region Members
    public static DungeonSurface Dungeon = null!;

    #endregion

    #region Constructor
    public InfoBar(DungeonSurface dungeon) : base(GameSettings.IbWidth, GameSettings.IbHeight)
    {
        Dungeon = dungeon;
        Position = GameSettings.IbPosition;
        FocusOnMouseClick = false;
    }
    #endregion
}
