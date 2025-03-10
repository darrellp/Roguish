﻿using Ninject;
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

    public StatusBar(GameSettings settings) : base(settings.SbWidth, settings.SbHeight)
    {
        ControlHost controls = [];
        Position = settings.SbPosition;

        var originString = string.Format(PositionFormat, 0, 0);
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
            Program.Kernel.Get<RootScreen>().FillSurface();
            controls.FocusedControl = null;
        };
        controls.Add(_btnRedraw);
        SadComponents.Add(controls);

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
}
