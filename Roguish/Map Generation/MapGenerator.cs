using GoRogue.MapGeneration;
using GoRogue.Random;
using SadRogue.Primitives.GridViews;
using ShaiRandom.Generators;

namespace Roguish.Map_Generation;
public class MapGenerator
{
    public static ISettableGridView<bool> WallFloorValues { get; set; } = null!;
    public static ScEntity?[,] ScEntityMap = new ScEntity?[GameSettings.DungeonWidth, GameSettings.DungeonHeight];

    public ISettableGridView<bool> Walls { get; set; } = null!;
    public Area[] Areas { get; set; } = null!;
    public static bool IsWalkable(int x, int y) => WallFloorValues[x, y];
    public static bool IsWalkable(Point pt) => WallFloorValues[pt];
    public bool Wall(int x, int y) => Walls[x, y];

    private readonly IEnhancedRandom _rng = GlobalRandom.DefaultRNG;


    public void Generate()
    {
        var generator = new Generator(GameSettings.DungeonWidth, GameSettings.DungeonHeight);
        // Add the steps to generate a map using the DungeonMazeMap built-in algorithm,
        // and generate the map.
        generator.ConfigAndGenerateSafe(gen =>
        {
            gen.AddStep(new RoomGenDAP() { MinRoomHeight = 5 });
            gen.AddStep(new RoomConnectDAP() { PctMergeChance = 30 });
        });

        WallFloorValues = generator.Context.GetFirst<ISettableGridView<bool>>("WallFloor");
        Walls = generator.Context.GetFirst<ISettableGridView<bool>>("Walls");
        Areas = generator.Context.GetFirst<Area[]>("Areas");
        ClearEntityMap();
    }

    public static void SetScEntityPosition(ScEntity scEntity, Point posOld, Point posNew)
    {
        ScEntityMap[posOld.X, posOld.Y] = null;
        ScEntityMap[posNew.X, posNew.Y] = scEntity;
    }

    public static void ClearEntityMap()
    {
        for (var iX = 0; iX < GameSettings.DungeonWidth; iX++)
        {
            for (var iY = 0; iY < GameSettings.DungeonHeight; iY++)
            {
                ScEntityMap[iX, iY] = null;
            }
        }
    }

    public static int BaseGlyphAt(int iX, int iY)
    {
        return IsWalkable(iX, iY) ? '.' : 0;
    }


    public Point FindRandomEmptyPoint()
    {
        while (true)
        {
            var x = _rng.NextInt(GameSettings.DungeonWidth);
            var y = _rng.NextInt(GameSettings.DungeonHeight);
            if (ScEntityMap[x, y] == null && IsWalkable(x, y))
            {
                return new Point(x, y);
            }
        }
    }


}

