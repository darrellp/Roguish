using System.Diagnostics;
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
    private static DungeonSurface? Dungeon { get; set; }
    private MapGenerator _mapgen;
    public bool DrawPath { get; set; }

    // ReSharper disable InconsistentNaming
    private static ColoredGlyph pathVert = new(Color.Yellow, Color.Black, 0xBA);
    private static ColoredGlyph pathHoriz = new(Color.Yellow, Color.Black, 0xCD);
    private static ColoredGlyph pathUR = new(Color.Yellow, Color.Black, 0xBB);
    private static ColoredGlyph pathUL = new(Color.Yellow, Color.Black, 0xC9);
    private static ColoredGlyph pathLR = new(Color.Yellow, Color.Black, 0xBC);
    private static ColoredGlyph pathLL = new(Color.Yellow, Color.Black, 0xC8);
    // ReSharper restore InconsistentNaming

    private static readonly Color wallColor = GameSettings.WallColor;
    private static readonly Color floorColor = GameSettings.FloorColor;
    private static readonly Color dimWallColor = Utility.DimmedColor(wallColor);
    private static readonly Color dimFloorColor = Utility.DimmedColor(floorColor);

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
    private readonly bool[,]_revealed;
    private bool _drawFov = true;

    public bool DrawFOV
    {
        get => _drawFov;
        set
        {
            if (_drawFov != value)
            {
                _drawFov = value;
                DrawMap();
            }
        }
    }

    public DungeonSurface(IEventSystem eventSystem, MapGenerator mapgen) : base(GameSettings.DungeonViewWidth, GameSettings.DungeonViewHeight, GameSettings.DungeonWidth, GameSettings.DungeonHeight)
    {
        // Create the entity renderer. This component should contain all the entities you want drawn on the surface
        _entityManager = new EntityManager();
        SadComponents.Add(_entityManager);
        _eventSystem = eventSystem;
        _mapgen = mapgen;
        _revealed = new bool[GameSettings.DungeonWidth, GameSettings.DungeonHeight];
        IsFocused = true;
        Dungeon = this;
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

    public ScEntity CreateScEntity(ColoredGlyph glyph, Point pt, int chGlyph, int zOrder)
    {
        var scEntity = new ScEntity(new ScEntity.SingleCell(glyph.Foreground, glyph.Background, chGlyph), zOrder)
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
        return _mapgen.Walkable(pt.X, pt.Y);
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

    public void MarkSeen(Point pt)
    {
        MarkFov(pt, true);
    }

    public void MarkUnseen(Point pt)
    {
        MarkFov(pt, false);
    }

    public void MarkFov(Point pt, bool fSeen)
    {
        _revealed[pt.X, pt.Y] = true;
        if (!DrawFOV)
        {
            return;
        }
        var (clrWall, clrFloor) = fSeen ? (wallColor, floorColor) : (dimWallColor, dimFloorColor);

        var glyph = this.GetCellAppearance(pt.X, pt.Y) as ColoredGlyph;
        Debug.Assert(glyph != null);
        if (glyph.Glyph == 0)
        {
            // Walls use bg color
            glyph.Background = clrWall;
        }
        else
        {
            glyph.Foreground = glyph.Glyph switch
            {
                '.' => clrFloor,
                _ => glyph.Foreground,          // Leave things alone if not specifically handled above
            };
        }
        DrawGlyph(glyph, pt);
    }

    public static void SignalNewFov(bool fDrawFullFov)
    {
        Debug.Assert(Dungeon != null, "Null dungeon in SignalNewFov");
        if (fDrawFullFov)
        {
            foreach (var point in Fov.CurrentFOV)
            {
                Dungeon.MarkSeen(point);
            }

            return;
        }
        foreach (var point in Fov.NewlySeen)
        {
            Dungeon.MarkSeen(point);
        }

        foreach (var point in Fov.NewlyUnseen)
        {
            Dungeon.MarkUnseen(point);
        }
    }

    public void FillSurface(DungeonSurface? surface)
    {
        // Create a new dungeon
        _mapgen.Generate();

        // Nothing has been revealed yet
        _revealed.Initialize();

        // Draw the new dungeon
        surface?.DrawMap(false);

        // Position stuff in the new dungeon
        _eventSystem.Publish(new NewDungeonEvent(0));

        // Make sure our hero is front and center
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

    private int GlyphAt(int iX, int iY)
    {
        return _mapgen.Walkable(iX, iY) ? '.' : 0;
    }

    public void DrawMap(bool fCenter = true)
    {
        this.Fill(new Rectangle(0, 0, Width, Height), DrawFOV ? Color.Black : floorColor, Color.Black, '.', Mirror.None);
        var offMapAppearance = new ColoredGlyph(Color.Black, Color.Black, 0x00);
        var wallAppearance = new ColoredGlyph(Color.Black, DrawFOV ? dimWallColor : wallColor, 0x00);
        var floorAppearance = new ColoredGlyph(DrawFOV ? dimFloorColor : floorColor, Color.Black, '.');
        for (var iX = 0; iX < Width; iX++)
        {
            for (var iY = 0; iY < Height; iY++)
            {
                if (DrawFOV && !_revealed[iX, iY])
                {
                    var appearance = new ColoredGlyph(Color.Black, Color.Black, GlyphAt(iX, iY));
                    DrawGlyph(appearance, iX, iY);
                    continue;
                }

                if (_mapgen.Wall(iX, iY))
                {
                    DrawGlyph(wallAppearance, iX, iY);
                }
                else if (_mapgen.Walkable(iX, iY))
                {
                    DrawGlyph(floorAppearance, iX, iY);
                }
                else
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