﻿using GoRogue.MapGeneration;
using SadConsole.StringParser;
using SadRogue.Primitives.GridViews;

namespace Roguish;
internal class MapGenerator
{
    public ISettableGridView<bool> WallFloorValues { get; init; }
    public bool Walkable(int x, int y) => WallFloorValues[x, y];

    public MapGenerator(int width, int height)
    {
        var generator = new Generator(width, height);
        // Add the steps to generate a map using the DungeonMazeMap built-in algorithm,
        // and generate the map.
        generator.ConfigAndGenerateSafe(gen =>
        {
            gen.AddSteps(DefaultAlgorithms.DungeonMazeMapSteps());
            //gen.AddSteps(DefaultAlgorithms.BasicRandomRoomsMapSteps());
            //gen.AddSteps(DefaultAlgorithms.CellularAutomataGenerationSteps());
        });

        WallFloorValues = generator.Context.GetFirst<ISettableGridView<bool>>("WallFloor");
    }

    public MapGenerator() : this(RootScreen.GetRootScreen().Width, RootScreen.GetRootScreen().Height) {}
}

