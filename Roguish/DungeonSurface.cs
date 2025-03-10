using Roguish.Map_Generation;
using SadConsole.Host;
using SadConsole.Input;
using SadRogue.Primitives.GridViews;
using Game = SadConsole.Game;

namespace Roguish
{
    internal class DungeonSurface : ScreenSurface
    {
        public DungeonSurface(int width, int height) : base(width, height)
        {
        }

        public DungeonSurface(int width, int height, ColoredGlyphBase[] initialCells) : base(width, height, initialCells)
        {
        }

        public DungeonSurface(int viewWidth, int viewHeight, int totalWidth, int totalHeight) : base(viewWidth, viewHeight, totalWidth, totalHeight)
        {
        }

        public DungeonSurface(IGridView<ColoredGlyphBase> surface, int visibleWidth = 0, int visibleHeight = 0) : base(surface, visibleWidth, visibleHeight)
        {
        }

        public DungeonSurface(int viewWidth, int viewHeight, int totalWidth, int totalHeight, ColoredGlyphBase[]? initialCells) : base(viewWidth, viewHeight, totalWidth, totalHeight, initialCells)
        {
        }

        public DungeonSurface(ICellSurface surface, IFont? font = null, Point? fontSize = null) : base(surface, font, fontSize)
        {
        }

        public DungeonSurface(GameSettings settings) : this(settings.DungeonWidth, settings.DungeonHeight)
        {
        }

        private readonly Point ptOffscreen = new Point(0, 0);
        private bool _offScreen = true;
        protected override void OnMouseMove(MouseScreenObjectState state)
        {
            var sb = StatusBar.GetStatusBar();
            sb.ReportMousePos(state.CellPosition);
        }

        public override void LostMouse(MouseScreenObjectState state)
        {
            var sb = StatusBar.GetStatusBar();
            sb.ReportMousePos(ptOffscreen);
        }
    }
}
