using Ninject;
using SadConsole.Input;

namespace Roguish;

public class DungeonSurface(GameSettings settings) : ScreenSurface(settings.DungeonWidth, settings.DungeonHeight)
{
    protected override void OnMouseMove(MouseScreenObjectState state)
    {
        var sb = Program.Kernel.Get<StatusBar>();
        sb.ReportMousePos(state.CellPosition);
    }

    public override void LostMouse(MouseScreenObjectState state)
    {
        var sb = Program.Kernel.Get<StatusBar>();
        sb.ReportMousePos(new Point(0, 0));
    }
}
