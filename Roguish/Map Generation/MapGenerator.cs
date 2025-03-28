using GoRogue.MapGeneration;
using SadRogue.Primitives.GridViews;

namespace Roguish.Map_Generation;
public class MapGenerator
{
    public ISettableGridView<bool> WallFloorValues { get; set; } = null!;
    public ISettableGridView<bool> Walls { get; set; } = null!;
    public Area[] Areas { get; set; } = null!;
    public bool Walkable(int x, int y) => WallFloorValues[x, y];
    public bool Wall(int x, int y) => Walls[x, y];

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
    }
}

