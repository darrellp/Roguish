using GoRogue.MapGeneration;
using SadRogue.Primitives.GridViews;

namespace Roguish;
internal class MapGenerator
{
    private ISettableGridView<bool> _wallFloorValues { get; init; }
    public bool Walkable(int x, int y) => _wallFloorValues[x, y];

    public MapGenerator(int width, int height)
    {
        var generator = new Generator(width, height);
        // Add the steps to generate a map using the DungeonMazeMap built-in algorithm,
        // and generate the map.
        generator.ConfigAndGenerateSafe(gen =>
        {
            gen.AddSteps(DefaultAlgorithms.DungeonMazeMapSteps());

        });

        _wallFloorValues = generator.Context.GetFirst<ISettableGridView<bool>>("WallFloor");
    }

    public MapGenerator() : this(RootScreen.GetRootScreen().Width, RootScreen.GetRootScreen().Height) {}
}

