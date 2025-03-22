using Ninject;
using SadConsole.UI.Controls;
using SystemsRx.ReactiveData;

// ReSharper disable IdentifierTypo

namespace Roguish;

internal class StatusBar : ScreenSurface
{
    #region Members
    public static Color BgndColor = Color.Blue;
    public static Color ForeColor = Color.White;
    public static string PositionFormat = "({0,3:D},{1,3:D})";
    public static ReactiveProperty<Point> MousePosition = new(new Point());
    public static DungeonSurface Dungeon = null!;
    #endregion

    #region Constructor
    public StatusBar(DungeonSurface dungeon) : base(GameSettings.SbWidth, GameSettings.SbHeight)
    {
        Dungeon = dungeon;
        Position = GameSettings.SbPosition;
        FocusOnMouseClick = false;
    }
    #endregion

    #region Handlers
    private Point _lastPosition = new(-1, -1);
    public void ReportMousePos(Point ptMouse)
    {
        if (ptMouse != _lastPosition)
        {
            MousePosition.SetValueAndForceNotify(ptMouse);
        }
    }

    public static EventHandler RedrawClick = (_, _) =>
    {
        Dungeon.FillSurface(Program.Kernel.Get<DungeonSurface>());
        Dungeon.IsFocused = true;
    };

    public static EventHandler DrawPathClick = (c, _) =>
    {
        var ds = Program.Kernel.Get<DungeonSurface>();
        ds.DrawPath = !((c as CheckBox)!).IsSelected;
        ds.DrawMap();
        Dungeon.IsFocused = true;
    };

    public static Action<Point> GetMousePosObserver(ControlBase c)
    {
        var label = c as Label;
        return (Point ptMouse) =>
        {
            var text = string.Format(PositionFormat, ptMouse.X, ptMouse.Y);
            label!.DisplayText = text;
        };
    }
    #endregion
}
