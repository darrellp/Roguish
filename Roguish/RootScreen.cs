//#define DRAWPATH

using Ninject;
using Roguish.Map_Generation;

// ReSharper disable IdentifierTypo

namespace Roguish;

public class RootScreen : ScreenObject
{
    private DungeonSurface _dungeonSurface;
    public int Width => _dungeonSurface.Width;
    public int Height => _dungeonSurface.Height;

    // ReSharper disable InconsistentNaming
    private static ColoredGlyph pathVert = new(Color.Red, Color.White, 0xBA);
    private static ColoredGlyph pathHoriz = new(Color.Red, Color.White, 0xCD);
    private static ColoredGlyph pathUR = new(Color.Red, Color.White, 0xBB);
    private static ColoredGlyph pathUL = new(Color.Red, Color.White, 0xC9);
    private static ColoredGlyph pathLR = new(Color.Red, Color.White, 0xBC);
    private static ColoredGlyph pathLL = new(Color.Red, Color.White, 0xC8);
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

    public RootScreen(GameSettings settings)
    {
        // Create the dungeon surface
        _dungeonSurface = Program.Kernel.Get<DungeonSurface>();

        // We can't FillSurface in the constructor because that will try to retrieve the singleton
        // RootScreen from IOC in the map generator which will cause an infinite recursive call and
        // consequent stack overflow.  
        //FillSurface();

        Children.Add(_dungeonSurface);
    }

    private void DrawGlyph(ColoredGlyph glyph, int x, int y)
    {
        _dungeonSurface.SetCellAppearance(x, y, glyph);
        _dungeonSurface.IsDirty = true;
    }

    private void DrawGlyph(ColoredGlyph glyph, Point pt)
    {
        DrawGlyph(glyph, pt.X, pt.Y);
    }

    public void FillSurface()
    {
        var settings = Program.Kernel.Get<GameSettings>();
        var wallAppearance = new ColoredGlyph(settings.ClearColor, Color.Black, 0x00);
        var areaAppearance = new ColoredGlyph(settings.ClearColor, Color.Chocolate, 0x00);

        _dungeonSurface.Fill(new Rectangle(0, 0, Width, Height), settings.ForeColor, settings.ClearColor, 0,
            Mirror.None);
        var gen = new MapGenerator();

        for (var iX = 0; iX < Width; iX++)
        {
            for (var iY = 0; iY < Height; iY++)
            {
                if (!gen.Walkable(iX, iY))
                {
                    DrawGlyph(wallAppearance, iX, iY);
                }
            }
        }

        foreach (var area in gen.Areas)
        {
            foreach (var pos in area.AsEnumerable())
            {
                DrawGlyph(areaAppearance, pos);
            }
        }

#if DRAWPATH
        var pathStart = new ColoredGlyph(Color.Green, Color.Green, '\u2591');
        var pathEnd = new ColoredGlyph(Color.Red, Color.Blue, '\u2591');

        var fFoundStart = false;
        var fFoundEnd = false;
        var ptStart = new Point();
        var ptEnd = new Point();
        for (var iX = 0; iX < Width; iX++)
        {
            for (var iY = 0; iY < Height; iY++)
            {
                if (!fFoundStart && gen.Walkable(iX, iY))
                {
                    ptStart = new Point(iX, iY);
                    fFoundStart = true;
                }
                if (!fFoundEnd && gen.Walkable(Width - 1 - iX, Height - 1 - iY))
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
        var aStar = new AStar(gen.WallFloorValues, Distance.Manhattan);
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
#else
    }
#endif

    //private static RootScreen? _rsSingleton;
    //public static RootScreen GetRootScreen()
    //{
    //    if (_rsSingleton == null)
    //    {
    //        throw new InvalidOperationException("Trying to retrieve singleton RootScreen before instantiation");
    //    }
    //    return _rsSingleton;
    //}

    // Solely for testing TUnit (as opposed to TUnit testing).
    public static int Add(int x, int y)
    {
        return x + y;
    }
}
