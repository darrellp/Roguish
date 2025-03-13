#define DRAWPATH
using GoRogue.Pathing;
using Ninject;
using Roguish.Map_Generation;
using SadConsole.Input;
// ReSharper disable IdentifierTypo

namespace Roguish;

public class DungeonSurface(GameSettings settings) : ScreenSurface(settings.DungeonWidth, settings.DungeonHeight)
{
    private static MapGenerator _mapgen;
    public bool DrawPath { get; set; }

    // ReSharper disable InconsistentNaming
    private static ColoredGlyph pathVert = new(Color.Yellow, Color.Black, 0xBA);
    private static ColoredGlyph pathHoriz = new(Color.Yellow, Color.Black, 0xCD);
    private static ColoredGlyph pathUR = new(Color.Yellow, Color.Black, 0xBB);
    private static ColoredGlyph pathUL = new(Color.Yellow, Color.Black, 0xC9);
    private static ColoredGlyph pathLR = new(Color.Yellow, Color.Black, 0xBC);
    private static ColoredGlyph pathLL = new(Color.Yellow, Color.Black, 0xC8);
    // ReSharper restore InconsistentNaming

    private static Dictionary<int, ColoredGlyph> _mpIndexToGlyph = new()
    {
        { 3, pathLL },
        { 5, pathVert },
        { 9, pathLR },
        { 6, pathUL },
        { 10, pathHoriz },
        { 12, pathUR },
    };

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

    private void DrawGlyph(ColoredGlyph glyph, int x, int y)
    {
        this.SetCellAppearance(x, y, glyph);
        IsDirty = true;
    }

    private void DrawGlyph(ColoredGlyph glyph, Point pt)
    {
        DrawGlyph(glyph, pt.X, pt.Y);
    }

    public static void FillSurface(DungeonSurface? surface)
    {
        _mapgen = new MapGenerator();
        surface?.DrawMap();
    }

    public void DrawMap()
    {
        var settings = Program.Kernel.Get<GameSettings>();
        this.Fill(new Rectangle(0, 0, Width, Height), settings.ForeColor, settings.ClearColor, '.',
            Mirror.None);
        var wallAppearance = new ColoredGlyph(settings.ClearColor, Color.DarkBlue, 0x00);
        var offMapAppearance = new ColoredGlyph(settings.ClearColor, Color.Black, 0x00);
        for (var iX = 0; iX < Width; iX++)
        {
            for (var iY = 0; iY < Height; iY++)
            {
                if (_mapgen.Wall(iX, iY))
                {
                    DrawGlyph(wallAppearance, iX, iY);
                }
                else if (!_mapgen.Walkable(iX, iY))
                {
                    DrawGlyph(offMapAppearance, iX, iY);
                }
            }
        }

        if (!DrawPath)
        {
            return;
        }

        var pathStart = new ColoredGlyph(Color.Green, Color.Green, '\u2591');
        var pathEnd = new ColoredGlyph(Color.Red, Color.Red, '\u2591');

        var fFoundStart = false;
        var fFoundEnd = false;
        var ptStart = new Point();
        var ptEnd = new Point();
        for (var iX = 0; iX < Width; iX++)
        {
            for (var iY = 0; iY < Height; iY++)
            {
                if (!fFoundStart && _mapgen.Walkable(iX, iY))
                {
                    ptStart = new Point(iX, iY);
                    fFoundStart = true;
                }
                if (!fFoundEnd && _mapgen.Walkable(Width - 1 - iX, Height - 1 - iY))
                {
                    ptEnd = new Point(Width - 1 - iX, Height - 1 - iY);
                    fFoundEnd = true;
                }
                if (fFoundStart && fFoundEnd)
                {
                    break;
                }
            }
            if (fFoundStart && fFoundEnd)
            {
                break;
            }
        }
        var aStar = new AStar(_mapgen.WallFloorValues, Distance.Manhattan);
        var path = aStar.ShortestPath(ptStart, ptEnd);
        if (path != null)
        {
            var pathSteps = path.Steps.ToArray();
            DrawGlyph(pathStart, ptStart);
            DrawGlyph(pathEnd, ptEnd);
            InscribePath(ptStart, pathSteps[0], pathSteps[1]);
            for (var i = 1; i <  pathSteps.Length - 1; i++)
            {
                InscribePath(pathSteps[i - 1], pathSteps[i], pathSteps[i + 1]);
            }
        }
    }


    private void InscribePath(Point prev, Point cur, Point next)
    {
        var index = ConnectValue(cur, prev) | ConnectValue(cur, next);
        DrawGlyph(_mpIndexToGlyph[index], cur);
    }

    private static int ConnectValue(Point pt, Point ptConnect)
    {
        // Connection points (with pt at the center):
        //     1
        //    +-+
        //   8| |2
        //    +-+
        //     4
        if (pt.X == ptConnect.X)
        {
            return pt.Y < ptConnect.Y ? 4 : 1;
        }
        else
        {
            return pt.X < ptConnect.X ? 2 : 8;
        }
    }
}