using System.Diagnostics;
using GoRogue.MapGeneration;
using GoRogue.Random;
using Roguish.ECS.Components;
using EcsRx.Extensions;
using SadRogue.Primitives.GridViews;
using ShaiRandom.Generators;
using Roguish.Screens;

namespace Roguish.Map_Generation;
public class MapGenerator
{
    public static ISettableGridView<bool> WallFloorValues { get; set; } = null!;
    public static int[,] AgentMap = new int[GameSettings.DungeonWidth, GameSettings.DungeonHeight];

    public ISettableGridView<bool> Walls { get; set; } = null!;
    public Area[] Areas { get; set; } = null!;
    public static bool IsWalkable(int x, int y) => WallFloorValues[x, y];
    public static bool IsWalkable(Point pt) => WallFloorValues[pt];
    public bool Wall(int x, int y) => Walls[x, y];

    private readonly IEnhancedRandom _rng = GlobalRandom.DefaultRNG;

    static MapGenerator()
    {
        ClearEntityMap();
    }

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

    public static void SetAgentPosition(int id, Point posOld, Point posNew)
    {
        AgentMap[posOld.X, posOld.Y] = -1;
        AgentMap[posNew.X, posNew.Y] = id;
    }

    public bool IsAgentAt(int x, int y)
    {
        return AgentMap[x, y] >= 0;
    }

    public bool IsAgentAt(Point pt)
    {
        return IsAgentAt(pt.X, pt.Y);
    }

    public static void ClearEntityMap()
    {
        for (var iX = 0; iX < GameSettings.DungeonWidth; iX++)
        {
            for (var iY = 0; iY < GameSettings.DungeonHeight; iY++)
            {
                AgentMap[iX, iY] = -1;
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
            if (!IsAgentAt(x, y) && IsWalkable(x, y))
            {
                return new Point(x, y);
            }
        }
    }

    public string GetDescription(Point pt)
    {
        var fovLevel = DungeonSurface.GetFov(pt);
        if (fovLevel == LevelOfFov.Unseen)
        {
            return "";
        }

        if (fovLevel == LevelOfFov.Lit && IsAgentAt(pt))
        {
            var ecsEntity = EcsApp.EntityDatabase.GetEntity(AgentMap[pt.X, pt.Y]);
            // ReSharper disable once InvertIf
            if (ecsEntity.HasComponent<AgentComponent>())
            {
                var agentCmp = ecsEntity.GetComponent<AgentComponent>();
                var agentInfo = AgentInfo.InfoFromType(agentCmp.AgentType);
                return agentInfo.Name;
            }
            return ecsEntity.HasComponent<IsPlayerControlledComponent>() ? "player" : "unknown agent";
        }
        else if (IsWalkable(pt))
        {
            return "floor";
        }
        else if (Wall(pt.X, pt.Y))
        {
            return "wall";
        }
        else
        {
            return "";
        }
    }
}

