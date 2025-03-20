﻿using Ninject;
using SadConsole.Host;
using Game = SadConsole.Game;
// ReSharper disable IdentifierTypo

namespace Roguish;
internal class TopContainer : ScreenObject
{
    public TopContainer()
    {
        UseMouse = false;
        if (GameSettings.FAllowResize)
        {
            // Resizing causes some weird gunk occasionally around the edges where the window is being resized into.
            // Suspicious that this is a MonoGame issue since there are some videos demonstrating similar behavior with
            // pure MonoGame.  Generally it can be reproed by making the window slightly smaller (less than a character)
            // and then dragging it larger.  Not sure what it is but pretty sure it's in MonoGame or perhaps SadConsole.
            // See some of the GIF examples here: https://docs.flatredball.com/gum/code/monogame/resizing-the-game-window
            Game.Instance.MonoGameInstance.Window.ClientSizeChanged += Game_WindowResized;
        }
    }

    private void Game_WindowResized(object? sender, EventArgs e)
    {
        var dungeonSurface = Program.Kernel.Get<DungeonSurface>();
        var chWidth = Game.Instance.MonoGameInstance.Window.ClientBounds.Width / dungeonSurface.FontSize.X;
        chWidth = Math.Min(GameSettings.DungeonWidth, Math.Max(80, chWidth));
        var chHeight = Game.Instance.MonoGameInstance.Window.ClientBounds.Height / dungeonSurface.FontSize.Y;
        chHeight = Math.Min(GameSettings.DungeonHeight, Math.Max(25, chHeight));
        var adjWidth = chWidth * dungeonSurface.FontSize.X;
        var adjHeight = chHeight * dungeonSurface.FontSize.Y;
        if (adjWidth != Game.Instance.MonoGameInstance.Window.ClientBounds.Width ||
            adjHeight != Game.Instance.MonoGameInstance.Window.ClientBounds.Height)
        {
            // Adjust window size to be a multiple of font size
            var gdm = Global.GraphicsDeviceManager;
            gdm.PreferredBackBufferWidth = adjWidth;
            gdm.PreferredBackBufferHeight = adjHeight;
            gdm.ApplyChanges();
        }

        var resizableDungeonSurface = (ICellSurfaceResize)dungeonSurface.Surface;
        resizableDungeonSurface.Resize(chWidth, chHeight - GameSettings.SbHeight, GameSettings.DungeonWidth, GameSettings.DungeonHeight, false);
        dungeonSurface.DrawMap();


        var sb = Program.Kernel.Get<StatusBar>();
        var resizableStatusBar = (ICellSurfaceResize)sb.Surface;
        sb.Position = new Point(0, chHeight - GameSettings.SbHeight);
        resizableStatusBar.Resize(chWidth, GameSettings.SbHeight, false);
    }

}
