using System.Collections.Generic;
using System.Text;
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
    private static readonly List<int>?[,] EntityMap = new List<int>?[GameSettings.DungeonWidth, GameSettings.DungeonHeight];
    public static ISettableGridView<bool> WalkableMap { get; private set; } = null!;
    internal static ISettableGridView<bool> RevealMap = new ArrayView2D<bool>(GameSettings.DungeonWidth, GameSettings.DungeonHeight);
    internal static ISettableGridView<bool> WallsMap { get; set; } = null!;

    private readonly IEnhancedRandom _rng = GlobalRandom.DefaultRNG;

    static MapGenerator()
    {
        ClearEntityMaps();
    }

    public static bool IsWalkable(int x, int y)
    {
        return WalkableMap[x, y];
    }

    public static bool IsWalkable(Point pt)
    {
        return WalkableMap[pt];
    }

    public bool Wall(int x, int y)
    {
        return WallsMap[x, y];
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

        WalkableMap = generator.Context.GetFirst<ISettableGridView<bool>>("WallFloor");
        WallsMap = generator.Context.GetFirst<ISettableGridView<bool>>("Walls");
        //Areas = generator.Context.GetFirst<Area[]>("Areas");
        ClearEntityMaps();
    }

    internal static void SetAgentPosition(int id, Point posOld, EcsType type, Point posNew)
    {
        if (type == EcsType.Agent || type == EcsType.Player)
        {
            AgentMap[posOld.X, posOld.Y] = -1;
            AgentMap[posNew.X, posNew.Y] = id;
        }
        else
        {
            EntityMap[posNew.X, posNew.Y] ??= [];
            EntityMap[posNew.X, posNew.Y]!.Add(id);
        }
    }

    private static bool IsEntityAt(int x, int y)
    {
        return AgentMap[x, y] >= 0 || EntityMap[x, y] != null;
    }

    private bool IsEntityAt(Point pt)
    {
        return IsEntityAt(pt.X, pt.Y);
    }

    ////////////////////////////////////////////////////////////////////////////////////////////////////
    /// <summary>   Gets entity and plurality at a position. </summary>
    ///
    /// <remarks>   If agent and items are located at the position then the agent will
    ///             be returned.  If more than one item exists then the boolean returned will
    ///             be true, otherwise false.  Null entity if there is nothing there.
    ///             Darrell Plank, 4/4/2025. </remarks>
    ///
    /// <param name="x">        The x coordinate. </param>
    /// <param name="y">        The y coordinate. </param>
    /// <param name="fItem">    (Optional) True to only return an item. </param>
    ///
    /// <returns>   The entity at the position and a bool indicating whether there is more than one entity there. </returns>
    ////////////////////////////////////////////////////////////////////////////////////////////////////

    internal List<EcsEntity> GetEntitiesAt(int x, int y, bool fItem = false)
    {
        var ret = new List<EcsEntity>();

        if (!fItem && IsAgentAt(x, y))
        {
            ret.Add(EcsApp.EntityDatabase.GetEntity(AgentMap[x, y]));
        }
        else if (EntityMap[x, y] != null)
        {
            ret.AddRange(EntityMap[x, y]!.Select(i => EcsApp.EntityDatabase.GetEntity(i)));
        }
        else
        {
            ret = [];
        }
        return ret;
    }

    ////////////////////////////////////////////////////////////////////////////////////////////////////
    /// <summary>   Gets entity and plurality at a position. </summary>
    ///
    /// <remarks>
    ///     If agent and items are located at the position then the agent will be returned.  If more
    ///     than one item exists then the boolean returned will be true, otherwise false.  Null
    ///     entity if there is nothing there. Darrell Plank, 4/4/2025.
    /// </remarks>
    ///
    /// <param name="pt">       The point. </param>
    /// <param name="fItem">    (Optional) True to only return an item. </param>
    ///
    /// <returns>
    ///     The entity at the position and a bool indicating whether there is more than one entity
    ///     there.
    /// </returns>
    ////////////////////////////////////////////////////////////////////////////////////////////////////

    internal List<EcsEntity> GetEntitiesAt(Point pt, bool fItem = false)
    {
        return GetEntitiesAt(pt.X, pt.Y, fItem);
    }

    ////////////////////////////////////////////////////////////////////////////////////////////////////
    /// <summary>   Removes the item at location from map. </summary>
    ///
    /// <remarks>   This only removes it from the MAP - it is still an entity in ECS and retains
    ///             whatever components it has including it's position/Display components.
    ///             Darrell Plank, 4/4/2025. </remarks>
    ///
    /// <param name="pt">   The point. </param>
    /// <param name="it">   The iterator. </param>
    ///
    /// <returns>   True if it succeeds, false if it fails. </returns>
    ////////////////////////////////////////////////////////////////////////////////////////////////////

    internal bool RemoveItemAt(Point pt, int it)
    {
        var items = EntityMap[pt.X, pt.Y];
        if (items == null || !items.Contains(it))
        {
            return false;
        }

        items.Remove(it);
        if (items.Count == 0)
        {
            EntityMap[pt.X, pt.Y] = null;
        }
        return true;
    }

    internal void RemoveAgentAt(Point pt)
    {
        AgentMap[pt.X, pt.Y] = -1;
    }


    internal static bool IsAgentAt(int x, int y)
    {
        return AgentMap[x, y] >= 0;
    }

    internal bool IsAgentAt(Point pt)
    {
        return IsAgentAt(pt.X, pt.Y);
    }

    internal static void ClearEntityMaps()
    {
        for (var iX = 0; iX < GameSettings.DungeonWidth; iX++)
        for (var iY = 0; iY < GameSettings.DungeonHeight; iY++)
        {
            AgentMap[iX, iY] = -1;
            EntityMap[iX, iY] = null;
        }
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
            if (!IsEntityAt(x, y) && IsWalkable(x, y)) return new Point(x, y);
        }
    }

    internal string GetDescription(Point pt)
    {
        var fovLevel = DungeonSurface.GetFov(pt);
        if (fovLevel == LevelOfFov.Unseen)
        {
            return """
                [c:r f:Yellow]????
                [c:r f:Orange]You peer into the inky abyss!
                """;
        }

        if (fovLevel == LevelOfFov.Lit && IsEntityAt(pt))
        {
            var entities = GetEntitiesAt(pt);
            var ecsEntity = entities[0];
            var type = ecsEntity.GetComponent<EntityTypeComponent>().EcsType;
            var ret = new StringBuilder();

            if (ecsEntity.HasComponent<DescriptionComponent>())
            {
                var descCmp = ecsEntity.GetComponent<DescriptionComponent>();
                var plural = entities.Count > 1 ? " and others" : "";
                ret = new StringBuilder( $"""
                        [c:r f:Yellow]{descCmp.Name} {plural}
                        [c:r f:Orange]{descCmp.Description}
                        """);
            }

            switch (type)
            {
                case EcsType.Agent:
                case EcsType.Player:
                    var healthCmp = ecsEntity.GetComponent<HealthComponent>();
                    ret.Append($"\n[c:r f:Orange]Hit Points: [c:r f:Cyan]{healthCmp.CurrentHealth}");
                    break;
            }

            return ret.ToString();
        }

        if (IsWalkable(pt))
            return """
                   [c:r f:Yellow]Floor
                   [c:r f:Orange]Just a boring old floor.
                   """;

        if (Wall(pt.X, pt.Y))
            return """
                   [c:r f:Yellow]Wall
                   [c:r f:Orange]Keeps bad stuff out and good stuff in
                   """;

        return """
               [c:r f:Yellow]Offscreen
               [c:r f:Orange]Nothing to see here - move along!
               """;
    }
}