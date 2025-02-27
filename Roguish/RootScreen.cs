using SadConsole.Host;
using Game = SadConsole.Game;

namespace Roguish;

public class RootScreen : ScreenObject
{
    private ScreenSurface _mainSurface;
    public int Width => _mainSurface.Width;
    public int Height => _mainSurface.Height;

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


    private void FillSurface()
    {
        var wallAppearance = new ColoredGlyph(GameSettings.ClearColor, Color.Black, '\u2591');

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
