using Ninject;
using Roguish.ECS.Systems;
using SadConsole.UI.Controls;
using SystemsRx.ReactiveData;

// ReSharper disable IdentifierTypo

namespace Roguish.Screens;

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
        Dungeon.FillSurface(Kernel.Get<DungeonSurface>());
        Dungeon.IsFocused = true;
    };

    public static EventHandler DrawPathClick = (c, _) =>
    {
        Dungeon.DrawPath = !(c as CheckBox)!.IsSelected;
        Dungeon.DrawMap();
        Dungeon.IsFocused = true;
        DungeonSurface.SignalNewFov(true);
    };

    public static Action<Point> GetMousePosObserver(object c)
    {
        var label = c as Label;
        return (ptMouse) =>
        {
            var text = string.Format(PositionFormat, ptMouse.X, ptMouse.Y);
            label!.DisplayText = text;
        };
    }

    public static void FovClick(object? sender, EventArgs e)
    {
        Dungeon.IsFocused = true;
        Dungeon.DrawFov = !Dungeon.DrawFov;
        if (!Dungeon.DrawFov)
        {
            foreach (var scEntity in Dungeon.GetEntities())
            {
                scEntity.IsVisible = true;
            }
        }
        else
        {
            foreach (var scEntity in Dungeon.GetEntities())
            {
                MovementSystem.DetermineVisibility(scEntity);
            }
        }
    }
    #endregion
}
