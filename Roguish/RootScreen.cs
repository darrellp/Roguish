﻿using GoRogue.Pathing;
using SadConsole.Host;
using Game = SadConsole.Game;

namespace Roguish;

public class RootScreen : ScreenObject
{
    private ScreenSurface _mainSurface;
    public int Width => _mainSurface.Width;
    public int Height => _mainSurface.Height;

    static ColoredGlyph pathVert = new ColoredGlyph(Color.Red, GameSettings.ClearColor, 0xB3);
    static ColoredGlyph pathHoriz = new ColoredGlyph(Color.Red, GameSettings.ClearColor, 0xC4);
    static ColoredGlyph pathUR = new ColoredGlyph(Color.Red, GameSettings.ClearColor, 0xBF);
    static ColoredGlyph pathUL = new ColoredGlyph(Color.Red, GameSettings.ClearColor, 0xDA);
    static ColoredGlyph pathLR = new ColoredGlyph(Color.Red, GameSettings.ClearColor, 0xD9);
    static ColoredGlyph pathLL = new ColoredGlyph(Color.Red, GameSettings.ClearColor, 0xC0);

    private static Dictionary<int, ColoredGlyph> mpIndexToGlyph = new Dictionary<int, ColoredGlyph>()
    {
        {3, pathLL},
        {5, pathVert},
        {9, pathLR},
        {6, pathUL},
        {10, pathHoriz},
        {12, pathUR},
    };



    public RootScreen()
    {
        _rsSingleton = this;

        // Create a surface that's the same size as the screen.
        _mainSurface = new ScreenSurface(GameSettings.GameWidth, GameSettings.GameHeight);

        FillSurface();

        // Add _mainSurface as a child object of this one. This object, RootScreen, is a simple object
        // and doesn't display anything itself. Since _mainSurface is going to be a child of it, _mainSurface
        // will be displayed.
        Children.Add(_mainSurface);
        if (GameSettings.FResizeHook)
        {
            // Resizing causes some weird gunk occasionally around the edges where the window is being resized into.
            // Suspicious that this is a MonoGame issue since there are some videos demonstrating similar behavior with
            // pure MonoGame.  Generally it can be reproed by making the window slightly smaller (less than a character)
            // and then dragging it larger.  Not sure what it is but pretty sure it's in MonoGame or perhaps SadConsole.
            // See some of the GIF examples here: https://docs.flatredball.com/gum/code/monogame/resizing-the-game-window
            Game.Instance.MonoGameInstance.Window.ClientSizeChanged += Game_WindowResized;
        }

    }

    void Game_WindowResized(object? sender, EventArgs e)
    {
        var rootConsole = _mainSurface;
        var resizableSurface = (ICellSurfaceResize)rootConsole.Surface;
        var chWidth = Game.Instance.MonoGameInstance.Window.ClientBounds.Width / rootConsole.FontSize.X;
        chWidth = Math.Max(80, chWidth);
        var chHeight = Game.Instance.MonoGameInstance.Window.ClientBounds.Height / rootConsole.FontSize.Y;
        chHeight = Math.Max(25, chHeight);
        var adjWidth = chWidth * rootConsole.FontSize.X;
        var adjHeight = chHeight * rootConsole.FontSize.Y;
        if (adjWidth != Game.Instance.MonoGameInstance.Window.ClientBounds.Width || adjHeight != Game.Instance.MonoGameInstance.Window.ClientBounds.Height)
        {
            // Adjust window size to be a multiple of font size
            var gdm = Global.GraphicsDeviceManager;
            gdm.PreferredBackBufferWidth = adjWidth;
            gdm.PreferredBackBufferHeight = adjHeight;
            gdm.ApplyChanges();
        }
        resizableSurface.Resize(chWidth, chHeight, false);
        FillSurface();
    }

    private void DrawGlyph(ColoredGlyph glyph, int x, int y)
    {
        _mainSurface.SetCellAppearance(x, y, glyph);
        _mainSurface.IsDirty = true;
    }

    private void DrawGlyph(ColoredGlyph glyph, Point pt)
    {
        DrawGlyph(glyph, pt.X, pt.Y);
    }

    private void FillSurface()
    {
        var wallAppearance = new ColoredGlyph(GameSettings.ClearColor, Color.Black, 0x00);//'\u2591');
        var pathStart = new ColoredGlyph(Color.Green, Color.Green, '\u2591');
        var pathEnd = new ColoredGlyph(Color.Red, Color.Red, '\u2591');

        _mainSurface.Fill(new Rectangle(0, 0, Width, Height), GameSettings.ForeColor, GameSettings.ClearColor, 0, Mirror.None);
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

        var yStartFirst = Enumerable.Range(0, Height - 1).First(y => gen.Walkable(1, y));
        var yStartLast = Enumerable.Range(1, Height - 1).Reverse().First(y => gen.Walkable(Width - 2, y));
        var ptStart = new Point(1, yStartFirst);
        var ptEnd = new Point(Width - 2, yStartLast);
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
        DrawGlyph(mpIndexToGlyph[index], cur);
    }

    private int ConnectValue(Point pt, Point ptConnect)
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


    private static RootScreen _rsSingleton = null;
    public static RootScreen GetRootScreen()
    {
        if (_rsSingleton == null)
        {
            throw new InvalidOperationException("Trying to retrieve singleton RootScreen before instantiation");
        }
        return _rsSingleton;
    }

    // Solely for testing TUnit (as opposed to TUnit testing).
    public static int Add(int x, int y)
    {
        return x + y;
    }
}
