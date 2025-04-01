using EcsRx.Extensions;
using GoRogue.MapGeneration;
using GoRogue.Random;
using Roguish.ECS.Components;
using Roguish.Screens;
using SadRogue.Primitives.GridViews;
using ShaiRandom.Generators;

namespace Roguish.Map_Generation;

// ReSharper disable once ClassNeverInstantiated.Global
public class MapGenerator
{
    private static readonly int[,] AgentMap = new int[GameSettings.DungeonWidth, GameSettings.DungeonHeight];

    private readonly IEnhancedRandom _rng = GlobalRandom.DefaultRNG;

    static MapGenerator()
    {
        ClearEntityMap();
    }

    public static ISettableGridView<bool> WallFloorValues { get; private set; } = null!;

    private ISettableGridView<bool> Walls { get; set; } = null!;
    //public Area[] Areas { get; set; } = null!;

    public static bool IsWalkable(int x, int y)
    {
        return WallFloorValues[x, y];
    }

    public static bool IsWalkable(Point pt)
    {
        return WallFloorValues[pt];
    }

    public bool Wall(int x, int y)
    {
        return Walls[x, y];
    }

    public void Generate()
    {
        var generator = new Generator(GameSettings.DungeonWidth, GameSettings.DungeonHeight);
        // Add the steps to generate a map using the DungeonMazeMap built-in algorithm,
        // and generate the map.
        generator.ConfigAndGenerateSafe(gen =>
        {
            gen.AddStep(new RoomGenDAP { MinRoomHeight = 5 });
            gen.AddStep(new RoomConnectDAP { PctMergeChance = 30 });
        });

        WallFloorValues = generator.Context.GetFirst<ISettableGridView<bool>>("WallFloor");
        Walls = generator.Context.GetFirst<ISettableGridView<bool>>("Walls");
        //Areas = generator.Context.GetFirst<Area[]>("Areas");
        ClearEntityMap();
    }

    internal static void SetAgentPosition(int id, Point posOld, Point posNew)
    {
        AgentMap[posOld.X, posOld.Y] = -1;
        AgentMap[posNew.X, posNew.Y] = id;
    }

    private bool IsAgentAt(int x, int y)
    {
        return AgentMap[x, y] >= 0;
    }

    private bool IsAgentAt(Point pt)
    {
        return IsAgentAt(pt.X, pt.Y);
    }

    private static void ClearEntityMap()
    {
        for (var iX = 0; iX < GameSettings.DungeonWidth; iX++)
        for (var iY = 0; iY < GameSettings.DungeonHeight; iY++)
            AgentMap[iX, iY] = -1;
    }

    internal static int BaseGlyphAt(int iX, int iY)
    {
        return IsWalkable(iX, iY) ? '.' : 0;
    }

    internal Point FindRandomEmptyPoint()
    {
        while (true)
        {
            var x = _rng.NextInt(GameSettings.DungeonWidth);
            var y = _rng.NextInt(GameSettings.DungeonHeight);
            if (!IsAgentAt(x, y) && IsWalkable(x, y)) return new Point(x, y);
        }
    }

    internal string GetDescription(Point pt)
    {
        var fovLevel = DungeonSurface.GetFov(pt);
        if (fovLevel == LevelOfFov.Unseen)
            return """
                   [c:r f:Yellow]????
                   [c:r f:Orange]You peer into the inky abyss!
                   """;

        if (fovLevel == LevelOfFov.Lit && IsAgentAt(pt))
        {
            var ecsEntity = EcsApp.EntityDatabase.GetEntity(AgentMap[pt.X, pt.Y]);
            // ReSharper disable once InvertIf
            if (ecsEntity.HasComponent<AgentComponent>())
            {
                var agentCmp = ecsEntity.GetComponent<AgentComponent>();
                var healthCmp = ecsEntity.GetComponent<HealthComponent>();
                var agentInfo = AgentInfo.InfoFromType(agentCmp.AgentType);
                return $"""
                        [c:r f:Yellow]{agentInfo.Name}
                        [c:r f:Orange]{agentInfo.Description}
                        [c:r f:Orange]Hit Points: [c:r f:Cyan]{healthCmp.CurrentHealth}
                        """;
            }

            const string strPlayer = """
                                     [c:r f:Yellow]Player
                                     [c:r f:Orange]It's you silly!
                                     """;
            const string strUnknown = """
                                      [c:r f:Yellow]Unknown Agent
                                      [c:r f:Orange]How did they sneak in here?
                                      """;
            return ecsEntity.HasComponent<IsPlayerControlledComponent>() ? strPlayer : strUnknown;
        }

        if (IsWalkable(pt))
            return """
                   [c:r f:Yellow]Floor
                   [c:r f:Orange]Just a boring old floor.
                   """;

        if (Wall(pt.X, pt.Y))
            return """
                   [c:r f:Yellow]Wall
                   [c:r f:Orange]To keep bad stuff out and good stuff in
                   """;

        return """
               [c:r f:Yellow]Offscreen
               [c:r f:Orange]Nothing to see here - move along!
               """;
    }
}