using Ninject;
using Roguish.Map_Generation;
using SadConsole.Host;
using SadConsole.Input;
using SadRogue.Primitives.GridViews;
using Game = SadConsole.Game;

namespace Roguish
{
    internal class DungeonSurface(GameSettings settings) : ScreenSurface(settings.DungeonWidth, settings.DungeonHeight)
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
}
