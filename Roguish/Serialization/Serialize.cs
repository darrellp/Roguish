using Newtonsoft.Json;
using System.Diagnostics;
using System.Text;
using Ninject;
using Roguish.ECS;
using Roguish.ECS.Components;
using Roguish.Map_Generation;
using Roguish.Screens;
using EcsRx.Extensions;
using Roguish.ECS.Systems;

// ReSharper disable IdentifierTypo

namespace Roguish.Serialization;
internal static partial class Serialize
{
    private static readonly string ComponentNamespace = typeof(HealthComponent).Namespace!;
    private static DungeonSurface Dungeon = Kernel.Get<DungeonSurface>();
    private static MapGenerator MapGen = Kernel.Get<MapGenerator>();

    internal static void SaveGame()
    {
        JsonSerializerSettings settings = new JsonSerializerSettings
        {
            ContractResolver = new CustomResolver("EcsComponent"),
            Formatting = Formatting.Indented,
            Converters = new List<JsonConverter>
            {
                new ReactiveConverter<Point>(),
                new ReactiveConverter<long>(),
            }
        };

        //PlayerSerialization(settings);
        var sb = new StringBuilder();
        var sw = new StringWriter(sb);

        using JsonWriter writer = new JsonTextWriter(sw);
        SerializeEcs(writer, settings);
        var jsonEcs = sb.ToString();

        // Given an ID get the index of the corresponding entity in ecsInfo
        var ecsInfo = DeserializeEcs(jsonEcs);

        //ReanimateHero(FindHero(ecsInfo), ecsInfo, mpOldIdToNewId);
        Reanimate(ecsInfo);
    }

    private static List<EcsEntity> CreateEntities(List<EntityInfo> ecsInfo, Dictionary<int, int> mpOldIdToNewId)
    {
        var collection = EcsApp.EntityDatabase.GetCollection();
        var newEntities = new List<EcsEntity>();

        foreach (var info in ecsInfo)
        {
            var entity = collection.CreateEntity();

            mpOldIdToNewId[info.OriginalId] = entity.Id;
            newEntities.Add(entity);
        }

        return newEntities;
    }

    private static int FindHero(List<EntityInfo> ecsInfo)
    {
        for (int iEntity = 0; iEntity < ecsInfo.Count; iEntity++)
        {
            var entityInfo = ecsInfo[iEntity];
            var info = entityInfo.Components.Where(c => c is IsPlayerControlledComponent).ToArray();
            if (info.Length > 0)
            {
                return iEntity;
            }
        }
        throw new Exception("No Hero found!");
    }

    private static void ReanimateHero(int iHero, List<EntityInfo> ecsInfo, Dictionary<int, int> mpOldIdToNewId)
    {
        var heroInfo = ecsInfo[iHero];
        MassageComponents(heroInfo, mpOldIdToNewId);

        var collection = EcsApp.EntityDatabase.GetCollection();
        var newHeroEntity = collection.CreateEntity();
        var newId = newHeroEntity.Id;
        var playerPos = EcsApp.PlayerPos;
        var oldPlayer = EcsRxApp.Player;
        MapGen.RemoveAgentAt(playerPos);
        Dungeon.RemoveScEntity(oldPlayer.GetComponent<DisplayComponent>().ScEntity);
        collection.RemoveEntity(EcsRxApp.Player.Id);
        MapGenerator.SetAgentPosition(newId, playerPos, EcsType.Player, playerPos);
        foreach (var cmp in heroInfo.Components)
        {
            newHeroEntity.AddComponent(cmp);
        }

        EcsRxApp.Player = newHeroEntity;
    }

    private static void Reanimate(List<EntityInfo> ecsInfo)
    {
        NewDungeonSystem.ClearLevel();
        // ClearLevel clears out everything except the player but we have to
        // remove him also
        RemovePlayer();

        var mpOldIdToNewId = new Dictionary<int, int>();
        var newEntities = CreateEntities(ecsInfo, mpOldIdToNewId);

        for (var iEntity = 0; iEntity < ecsInfo.Count; iEntity++)
        {
            var entityInfo = ecsInfo[iEntity];
            var newEntity = newEntities[iEntity];
            MassageComponents(entityInfo, mpOldIdToNewId);
            if (entityInfo.FindComponent<IsPlayerControlledComponent>() != null)
            {
                EcsRxApp.Player = newEntity;
            }

            // We want to set position last since it triggers MovementSystem which 
            // in turn relies on other components to work properly
            EcsComponent? positionComponent = null;

            foreach (var cmp in entityInfo.Components)
            {
                if (cmp is PositionComponent)
                {
                    positionComponent = cmp;
                    continue;
                }
                newEntity.AddComponent(cmp);
            }

            if (positionComponent != null)
            {
                newEntity.AddComponent(positionComponent);
            }
        }
    }

    private static void RemovePlayer()
    {
        var playerPos = EcsApp.PlayerPos;
        var oldPlayer = EcsRxApp.Player;
        var collection = EcsApp.EntityDatabase.GetCollection();

        MapGen.RemoveAgentAt(playerPos);
        Dungeon.RemoveScEntity(oldPlayer.GetComponent<DisplayComponent>().ScEntity);
        collection.RemoveEntity(EcsRxApp.Player.Id);

    }

    private static void MassageComponents(EntityInfo entityInfo, Dictionary<int, int> mpOldIdToNewId)
    {
        foreach (var cmp in entityInfo.Components) 
        {
            switch (cmp)
            {
                case DisplayComponent displayComponent:
                    displayComponent.ScEntity = Dungeon.GetScEntity(entityInfo);
                    break;

                case EquippedComponent equippedComponent:
                    equippedComponent.RemapEquipment(mpOldIdToNewId);
                    break;
            }
        }
    }

    internal static int RemapId(int originalId, Dictionary<int, int> mpOldIdToNewId)
    {
        return mpOldIdToNewId.GetValueOrDefault(originalId, originalId);
    }

    private static List<EntityInfo> DeserializeEcs(string json)
    {
        var infoList = new List<EntityInfo>();
        var reader = new JsonTextReader(new StringReader(json));
        reader.Read();
        Debug.Assert(reader.TokenType == JsonToken.StartArray);
        var index = 0;
        while (reader.Read())       // Start of object
        {
            if (reader.TokenType == JsonToken.EndArray)
                break;
            var entityInfo = DeserializeEntity(reader);
            infoList.Add(entityInfo);
        }

        return infoList;
    }

    private static EntityInfo DeserializeEntity(JsonReader reader)
    {
        var componentList = new List<EcsComponent>();
        reader.Read();  // OriginalId:
        reader.Read();  // <Id value>
        var originalId = Convert.ToInt32(reader.Value!);
        reader.Read();  // Components:
        reader.Read();  // StartArray
        Debug.Assert(reader.TokenType == JsonToken.StartArray);

        // Components
        while (true)
        {
            var cmp = DeserializeComponent(reader);
            if (cmp == null)
            {
                break;
            }
            componentList.Add(cmp);
        }

        return new EntityInfo(originalId, componentList);
    }

    private static EcsComponent? DeserializeComponent(JsonReader reader)
    {
        // I don't understand why this read doesn't come back with StartObject.  I'm happy it
        // doesn't because it gives me exactly what I really want but I still don't understand.
        reader.Read();
        if (reader.TokenType == JsonToken.EndArray)
        {
            reader.Read();      // EndObject
            return null;
        }

        return DeserializeComponent((reader.Value as string)!);
    }

    private static EcsComponent? DeserializeComponent(string json)
    {
        var reader = new JsonTextReader(new StringReader(json));
        reader.Read();      // StartObject or EndArray
        if (reader.TokenType == JsonToken.EndArray)
        {
            return null;
        }
        reader.Read();      // "ComponentType"
        reader.Read();      // <ComponentType value (i.e., typename)>
        var typeName = ComponentNamespace + "." + reader.Value;
        var type = Type.GetType(typeName);
        Debug.Assert(type != null);

        // Use the type to deserialize
        return (EcsComponent?)JsonConvert.DeserializeObject(json, type);
    }

    #region Serialization
    private static void SerializeEcs(JsonWriter writer, JsonSerializerSettings settings)
    {
        writer.WriteStartArray();
        foreach (var entity in EcsApp.EntityDatabase.GetCollection())
        {
            SerializeEntity(entity, writer, settings);
        }
        writer.WriteEndArray();
    }

    private static void SerializeEntity(EcsEntity entity, JsonWriter writer, JsonSerializerSettings settings)
    {
        writer.WriteStartObject();
        writer.WritePropertyName("OriginalId");
        writer.WriteValue(entity.Id);
        writer.WritePropertyName("Components");
        writer.WriteStartArray();
        foreach (var cmp in entity.Components)
        {
            var json = JsonConvert.SerializeObject(cmp, settings);
            writer.WriteValue(json);
        }
        writer.WriteEndArray();
        writer.WriteEndObject();
    }
    #endregion
}
