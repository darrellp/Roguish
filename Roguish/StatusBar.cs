using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SadConsole.UI;
using SadConsole.UI.Controls;

namespace Roguish;

internal class StatusBar : ScreenSurface
{
    public static Color BgndColor = Color.Blue;
    public static Color ForeColor = Color.White;
    public static string PositionFormat = "({0,4:D},{1,4:D})";

    private Button _btnRedraw;
    private Label _lblPosition;

    public StatusBar(int width, int height) : base(width, height)
    {
        _sbSingleton = this;
        ControlHost controls = new();

        var originString = String.Format(PositionFormat, 0, 0);
        var pointWidth = originString.Length;

        _lblPosition = new Label(pointWidth)
        {
            Position = new Point(0, 0),
            DisplayText = "Testing"
        };
        controls.Add(_lblPosition);

        _btnRedraw = new(10, 1)
        {
            Position = new Point(pointWidth + 3, 0),
            Text = "Redraw",
            FocusOnMouseClick = false,
        };

        _btnRedraw.Click += (s, e) =>
        {
            RootScreen.GetRootScreen().FillSurface();
            controls.FocusedControl = null;
        };
        controls.Add(_btnRedraw);
        SadComponents.Add(controls);


        if (GameSettings.FResizeHook)
        {
            // Resizing causes some weird gunk occasionally around the edges where the window is being resized into.
            // Suspicious that this is a MonoGame issue since there are some videos demonstrating similar behavior with
            // pure MonoGame.  Generally it can be reproed by making the window slightly smaller (less than a character)
            // and then dragging it larger.  Not sure what it is but pretty sure it's in MonoGame or perhaps SadConsole.
            // See some of the GIF examples here: https://docs.flatredball.com/gum/code/monogame/resizing-the-game-window
            //Game.Instance.MonoGameInstance.Window.ClientSizeChanged += Game_WindowResized;
        }

        ReportMousePos(new Point());
    }

    private Point lastPosition = new Point(-1, -1);
    public void ReportMousePos(Point ptMouse)
    {
        if (ptMouse != lastPosition)
        {
            lastPosition = ptMouse;
            var text = String.Format(PositionFormat, ptMouse.X, ptMouse.Y);
            _lblPosition.DisplayText = text;
        }
    }

    private static StatusBar? _sbSingleton;

    public static StatusBar GetStatusBar()
    {
        if (_sbSingleton == null)
        {
            throw new InvalidOperationException("Trying to retrieve singleton StatusBar before instantiation");
        }
        return _sbSingleton;
    }
}
