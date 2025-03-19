#define DRAWPATH
using GoRogue.Pathing;
using GoRogue.Random;
using Ninject;
using Roguish.ECS.Events;
using Roguish.Map_Generation;
using SadConsole.Entities;
using SadConsole.Input;
using ShaiRandom.Generators;
using SystemsRx.Events;

// ReSharper disable IdentifierTypo

namespace Roguish;

public class DungeonSurface : ScreenSurface
{
    private static MapGenerator? _mapgen = null;
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

    private readonly EntityManager _entityManager;
    private readonly IEnhancedRandom _rng = GlobalRandom.DefaultRNG;
    private static IEventSystem _eventSystem = null!;

    public DungeonSurface(IEventSystem eventSystem) : base(GameSettings.DungeonViewWidth, GameSettings.DungeonViewHeight, GameSettings.DungeonWidth, GameSettings.DungeonHeight)
    {
        // Create the entity renderer. This component should contain all the entities you want drawn on the surface
        _entityManager = new EntityManager();
        SadComponents.Add(_entityManager);
        _eventSystem = eventSystem;
        IsFocused = true;
    }

    public override bool ProcessKeyboard(Keyboard keyboard)
    {
        if (!keyboard.HasKeysPressed)
        {
            return false;
        }
        _eventSystem.Publish(new KeyboardEvent(keyboard.KeysPressed));
        KeepPlayerInView();
        return true;
    }

    public Entity CreateScEntity(ColoredGlyph glyph, Point pt, int chGlyph, int zOrder)
    {
        var scEntity = new Entity(new Entity.SingleCell(glyph.Foreground, glyph.Background, chGlyph), zOrder)
        {
            Position = pt,
        };
        _entityManager.Add(scEntity);
        return scEntity;
    }

    public void RemoveScEntity(ScEntity scEntity)
    {
        _entityManager.Remove(scEntity);
    }

    public Point FindRandomEmptyPoint()
    {
        if (_mapgen == null)
        {
            throw new InvalidOperationException("FindRandomEmptyPoint called before map generation");
        }
        while (true)
        {
            var x = _rng.NextInt(Width);
            var y = _rng.NextInt(Height);

            if (_mapgen.Walkable(x, y))
            {
                return new Point(x, y);
            }
        }
    }

    public bool IsWalkable(Point pt)
    {
        return _mapgen!.Walkable(pt.X, pt.Y);
    }

    protected override void OnMouseMove(MouseScreenObjectState state)
    {
        var sb = Program.Kernel.Get<StatusBar>();
        sb.ReportMousePos(state.CellPosition);// + ViewPosition);
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

    public void FillSurface(DungeonSurface? surface)
    {
        _mapgen = new MapGenerator();
        surface?.DrawMap(false);
        _eventSystem.Publish(new LevelChangeEvent(0));
        CenterView(Program.EcsApp.PlayerPos);
    }

    private void CenterView(Point pt)
    {
        var idealPt = pt - new Point(ViewWidth / 2, ViewHeight / 2);
        var x = Math.Min(GameSettings.DungeonWidth - ViewWidth, (Math.Max(idealPt.X, 0)));
        var y = Math.Min(GameSettings.DungeonHeight - ViewHeight, (Math.Max(idealPt.Y, 0)));
        var newPos = new Point(x, y);
        ViewPosition = newPos;
    }

    private void KeepPlayerInView()
    {
        var playerPos = Program.EcsApp.PlayerPos;
        var playerPosRelative = playerPos - ViewPosition;
        var (x, y) =  ViewPosition;
        var isChanged = false;
        if (playerPosRelative.X < GameSettings.BorderWidthX)
        {
            isChanged = true;
            x -= GameSettings.BorderWidthX - playerPosRelative.X;
        }
        else if (playerPosRelative.X >= ViewWidth - GameSettings.BorderWidthX)
        {
            isChanged = true;
            x -= ViewWidth - GameSettings.BorderWidthX - playerPosRelative.X;
        }
        if (playerPosRelative.Y < GameSettings.BorderWidthY)
        {
            isChanged = true;
            y -= GameSettings.BorderWidthY - playerPosRelative.Y;
        }
        else if (playerPosRelative.Y >= ViewHeight - GameSettings.BorderWidthY)
        {
            isChanged = true;
            y -= ViewHeight - GameSettings.BorderWidthY - playerPosRelative.Y;
        }

        if (isChanged)
        {
            ViewPosition = new Point(x, y);
        }
    }

    public void DrawMap(bool fCenter = true)
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
                if (_mapgen!.Wall(iX, iY))
                {
                    DrawGlyph(wallAppearance, iX, iY);
                }
                else if (!_mapgen.Walkable(iX, iY))
                {
                    DrawGlyph(offMapAppearance, iX, iY);
                }
            }
        }

        if (fCenter)
        {
            CenterView(Program.EcsApp.PlayerPos);
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
                if (!fFoundStart && _mapgen!.Walkable(iX, iY))
                {
                    ptStart = new Point(iX, iY);
                    fFoundStart = true;
                }
                if (!fFoundEnd && _mapgen!.Walkable(Width - 1 - iX, Height - 1 - iY))
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
        var aStar = new AStar(_mapgen!.WallFloorValues, Distance.Manhattan);
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