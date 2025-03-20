using GoRogue.MapGeneration;
using SadRogue.Primitives.GridViews;

namespace Roguish.Map_Generation;
internal class MapGenerator
{
    public ISettableGridView<bool> WallFloorValues { get; init; }
    public ISettableGridView<bool> Walls { get; init; }
    public Area[] Areas { get; init; }
    public bool Walkable(int x, int y) => WallFloorValues[x, y];
    public bool Wall(int x, int y) => Walls[x, y];

    public MapGenerator(int width, int height)
    {
        var generator = new Generator(width, height);
        // Add the steps to generate a map using the DungeonMazeMap built-in algorithm,
        // and generate the map.
        generator.ConfigAndGenerateSafe(gen =>
        {
            //gen.AddSteps(DefaultAlgorithms.DungeonMazeMapSteps());
            //gen.AddSteps(DefaultAlgorithms.BasicRandomRoomsMapSteps());
            //gen.AddSteps(DefaultAlgorithms.CellularAutomataGenerationSteps());
            gen.AddStep(new RoomGenDAP() { MinRoomHeight = 4, MinRoomWidth = 10, SuperCellHeight = 12, SuperCellWidth = 20});
            gen.AddStep(new RoomConnectDAP() {PctMergeChance = 40});
        });

        WallFloorValues = generator.Context.GetFirst<ISettableGridView<bool>>("WallFloor");
        Walls = generator.Context.GetFirst<ISettableGridView<bool>>("Walls");
        Areas = generator.Context.GetFirst<Area[]>("Areas");
    }

    public MapGenerator() : this(GameSettings.DungeonWidth, GameSettings.DungeonHeight) {}
    
}

