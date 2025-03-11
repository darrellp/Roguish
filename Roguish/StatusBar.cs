using Ninject;
using SadConsole.UI;
using SadConsole.UI.Controls;
// ReSharper disable IdentifierTypo

namespace Roguish;

internal class StatusBar : ScreenSurface
{
    public static Color BgndColor = Color.Blue;
    public static Color ForeColor = Color.White;
    public static string PositionFormat = "({0,3:D},{1,3:D})";

    private Button _btnRedraw;
    private Label _lblPosition;
    private CheckBox _chkDrawPath;

    public StatusBar(GameSettings settings) : base(settings.SbWidth, settings.SbHeight)
    {
        ControlHost controls = [];
        Position = settings.SbPosition;

        var originString = string.Format(PositionFormat, 0, 0);
        var pointWidth = originString.Length;

        _lblPosition = new Label(pointWidth)
        {
            Position = new Point(),
        };
        controls.Add(_lblPosition);

        _btnRedraw = new Button(width:10)
        {
            Position = new Point(pointWidth + 3, 0),
            Text = "Redraw",
            FocusOnMouseClick = false,
        };

        _btnRedraw.Click += (_, _) =>
        {
            Program.Kernel.Get<DungeonSurface>().FillSurface();
        };
        controls.Add(_btnRedraw);

        _chkDrawPath = new CheckBox("Draw Path")
        {
            Position = new Point(pointWidth + 3 + _btnRedraw.Width + 3, 0),
            FocusOnMouseClick = false,
        };
        _chkDrawPath.Click += (c, _) =>
        {
            var ds = Program.Kernel.Get<DungeonSurface>();
            ds.DrawPath = !((c as CheckBox)!).IsSelected;
            ds.DrawMap();
        };

        controls.Add(_chkDrawPath);

        SadComponents.Add(controls);

        ReportMousePos(new Point());
    }

    private Point _lastPosition = new(-1, -1);
    public void ReportMousePos(Point ptMouse)
    {
        if (ptMouse != _lastPosition)
        {
            _lastPosition = ptMouse;
            var text = string.Format(PositionFormat, ptMouse.X, ptMouse.Y);
            _lblPosition.DisplayText = text;
        }
    }
}
