using SystemsRx.ReactiveData;

namespace Roguish.Screens;
internal class InfoBar : ScreenSurface
{
    #region Members
    public static DungeonSurface Dungeon = null!;
    public static ReactiveProperty<string> Description = new("Testing!");

    #endregion

    #region Constructor
    public InfoBar(DungeonSurface dungeon) : base(GameSettings.IbWidth, GameSettings.IbHeight)
    {
        Dungeon = dungeon;
        Position = GameSettings.IbPosition;
        FocusOnMouseClick = false;
    }
    #endregion

    #region Handlers
    public void SetDescription(string description)
    {
        Description.SetValueAndForceNotify(description);
    }
    #endregion
}
