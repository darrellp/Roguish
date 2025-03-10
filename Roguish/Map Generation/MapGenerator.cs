using GoRogue.MapGeneration;
using Ninject;
using SadRogue.Primitives.GridViews;

namespace Roguish.Map_Generation;
internal class MapGenerator
{
    public ISettableGridView<bool> WallFloorValues { get; init; }
    public Area[] Areas { get; init; }
    public bool Walkable(int x, int y) => WallFloorValues[x, y];

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
            gen.AddStep(new RoomGenDAP() { MinRoomHeight = 5});
            gen.AddStep(new RoomConnectDAP() {PctMergeChance = 30});
        });

        WallFloorValues = generator.Context.GetFirst<ISettableGridView<bool>>("WallFloor");
        Areas = generator.Context.GetFirst<Area[]>("Areas");
    }

    public MapGenerator() : this(Program.Kernel.Get<DungeonSurface>().Width, Program.Kernel.Get<DungeonSurface>().Height) {}
}

