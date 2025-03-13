using Ninject;
using SadConsole.UI.Controls;
using SystemsRx.ReactiveData;

// ReSharper disable IdentifierTypo

namespace Roguish;

internal class StatusBar : ScreenSurface
{
    public static Color BgndColor = Color.Blue;
    public static Color ForeColor = Color.White;
    public static string PositionFormat = "({0,3:D},{1,3:D})";
    public static ReactiveProperty<Point> MousePosition = new(new Point());

    public StatusBar(GameSettings settings) : base(settings.SbWidth, settings.SbHeight)
    {
        Position = settings.SbPosition;
    }

    private Point _lastPosition = new(-1, -1);
    public void ReportMousePos(Point ptMouse)
    {
        if (ptMouse != _lastPosition)
        {
            MousePosition.SetValueAndForceNotify(ptMouse);
        }
    }

    #region Handlers
    public static EventHandler RedrawClick = (c, _) =>
    {
        DungeonSurface.FillSurface(Program.Kernel.Get<DungeonSurface>());
    };

    public static EventHandler DrawPathClick = (c, _) =>
    {
        var ds = Program.Kernel.Get<DungeonSurface>();
        ds.DrawPath = !((c as CheckBox)!).IsSelected;
        ds.DrawMap();
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
