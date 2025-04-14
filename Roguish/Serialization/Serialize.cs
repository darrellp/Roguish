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
using SadConsole.SerializedTypes;
using SadRogue.Primitives.GridViews;
using Roguish.ECS.Events;

// ReSharper disable IdentifierTypo

namespace Roguish.Serialization;
internal static partial class Serialize
{
    #region private fields
    private static readonly string ComponentNamespace = typeof(HealthComponent).Namespace!;
    private static readonly DungeonSurface Dungeon = Kernel.Get<DungeonSurface>();
    private static readonly MapGenerator MapGen = Kernel.Get<MapGenerator>();
    private static readonly JsonSerializerSettings Settings;
    #endregion

    #region Static Constructor
    static Serialize()
    {
        Settings = new JsonSerializerSettings
        {
            ContractResolver = new CustomResolver("EcsComponent"),
            Formatting = Formatting.Indented,
            Converters = new List<JsonConverter>
            {
                new ReactiveConverter<Point>(),
                new ReactiveConverter<long>(),
                new ColorJsonConverter(),
                new ColoredGlyphJsonConverter(),
                new ScEntityConverter(Dungeon),
            }
        };
    }
    #endregion

    public static void Test()
    {
        var json = SerializeGame();
        DeserializeGame(json);
    }

    internal static void SaveGame()
    {
        var json = SerializeGame();
    }

    #region Deserialization
    public static void DeserializeGame(string json)
    {
        // Toss everything!
        EraseLevel();
        using JsonReader reader = new JsonTextReader(new StringReader(json));
        DeserializeMaps(reader);
        DeserializeEcs(reader);
    }

    private static void EraseLevel()
    {
        foreach (var item in EcsApp.EntityDatabase.GetCollection().ToArray())
        {
            if (item.HasComponent<DisplayComponent>())
            {
                var displayCmp = item.GetComponent(typeof(DisplayComponent)) as DisplayComponent;
                Dungeon.RemoveScEntity(displayCmp!.ScEntity);
            }
            EcsApp.EntityDatabase.GetCollection().RemoveEntity(item.Id);
        }
        MapGenerator.ClearEntityMaps();
    }

    private static void DeserializeEcs(JsonReader reader)
    {
        var infoList = new List<EntityInfo>();
        reader.Read();
        Debug.Assert(reader.TokenType == JsonToken.StartArray);
        while (reader.Read())       // Start of object
        {
            if (reader.TokenType == JsonToken.EndArray)
                break;
            var entityInfo = DeserializeEntity(reader);
            infoList.Add(entityInfo);
        }

        DeserializeFromInfo(infoList);
    }

    private static void DeserializeFromInfo(List<EntityInfo> ecsInfo)
    {
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

            // Do this last so MovementSystem has all the info it needs
            if (positionComponent != null)
            {
                newEntity.AddComponent(positionComponent);
            }

            // If our entity is equipped or in the backpack we don't want his sprite to appear in the dungeon
            if (newEntity.HasComponent<DisplayComponent>() && 
                (newEntity.HasComponent<InBackpackComponent>() || newEntity.HasComponent<IsEquippedComponent>()))
            {
                Dungeon.RemoveScEntity(newEntity.GetComponent<DisplayComponent>().ScEntity);
            }
        }

        Dungeon.SetVisibilities();
    }

    private static EntityInfo DeserializeEntity(JsonReader reader)
    {
        var componentList = new List<EcsComponent>();
        reader.Read();  // OriginalId:https://library.umbc.edu/specialcollections/garrett/wp-content/uploads/sites/5/2024/06/What-Gef-is-pg-44-scaled.jpg
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
        return (EcsComponent?)JsonConvert.DeserializeObject(json, type, Settings);
    }

    private static void MassageComponents(EntityInfo entityInfo, Dictionary<int, int> mpOldIdToNewId)
    {
        foreach (var cmp in entityInfo.Components)
        {
            switch (cmp)
            {
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
    
    private static void DeserializeMaps(JsonReader reader)
    {
        reader.Read();      // Start object

        // The agent map and entity map are filled as a consequence of filling
        // the ECS entities so are not serialized explicitly here.
        DeserializeMap(MapGenerator.RevealMap, "RevealMap", reader);
        DeserializeMap(MapGenerator.WalkableMap, "WalkableMap", reader);
        DeserializeMap(MapGenerator.WallsMap, "WallsMap", reader);
        reader.Read();      // End object
    }

    private static void DeserializeMap(ISettableGridView<bool> map, string property, JsonReader reader)
    {
        reader.Read();      // Property name
        reader.Read();      // The map
        var packed = JsonConvert.DeserializeObject<long[]>(reader.Value as string);
        UnpackBoolArray(map, packed, GameSettings.DungeonWidth, GameSettings.DungeonHeight);
    }
    #endregion

    #region Serialization
    public static string SerializeGame()
    {
        var sb = new StringBuilder();
        var sw = new StringWriter(sb);

        using JsonWriter writer = new JsonTextWriter(sw);
        writer.WriteStartObject();
        SerializeMaps(writer);
        writer.WritePropertyName("EcsEntities");
        SerializeEcs(writer);
        writer.WriteEndObject();
        var json = sb.ToString();
        return json;
    }

    private static void SerializeEcs(JsonWriter writer)
    {
        writer.WriteStartArray();
        foreach (var entity in EcsApp.EntityDatabase.GetCollection())
        {
            SerializeEntity(entity, writer);
        }
        writer.WriteEndArray();
    }

    private static void SerializeEntity(EcsEntity entity, JsonWriter writer)
    {
        writer.WriteStartObject();
        writer.WritePropertyName("OriginalId");
        writer.WriteValue(entity.Id);
        writer.WritePropertyName("Components");
        writer.WriteStartArray();
        foreach (var cmp in entity.Components)
        {
            var json = JsonConvert.SerializeObject(cmp, Settings);
            writer.WriteValue(json);
        }
        writer.WriteEndArray();
        writer.WriteEndObject();
    }
    private static void SerializeMaps(JsonWriter writer)
    {
        // The agent map and entity map are filled as a consequence of filling
        // the ECS entities so are not serialized explicitly here.
        SerializeMap(MapGenerator.RevealMap, "RevealMap", writer);
        SerializeMap(MapGenerator.WalkableMap, "WalkableMap", writer);
        SerializeMap(MapGenerator.WallsMap, "WallsMap", writer);
    }

    private static void SerializeMap(ISettableGridView<bool> map, string property, JsonWriter writer)
    {
        var packed = PackBoolArray(map);
        writer.WritePropertyName(property);
        var json = JsonConvert.SerializeObject(packed);
        writer.WriteValue(json);
    }
    #endregion

    #region Packing/Unpacking
    private static long[] PackBoolArray(ISettableGridView<bool> array)
    {
        var ret = new long[(array.Width * array.Height + 63) / 64];
        var bitIndex = 0;
        var arrayIndex = 0;
        var current = 0L;
        for (var iRow = 0; iRow < array.Height; iRow++)
        {
            for (var iCol = 0; iCol < array.Width; iCol++)
            {
                if (array[iCol, iRow])
                {
                    current |= 1L << bitIndex;
                }
                if (++bitIndex == 64)
                {
                    bitIndex = 0;
                    ret[arrayIndex++] = current;
                    current = 0;
                }
            }
        }

        if (bitIndex != 0)
        {
            ret[arrayIndex] = current;
        }

        return ret;
    }

    private static void UnpackBoolArray(ISettableGridView<bool> map, long[] array, int width, int height)
    {
        var ret = new ArrayView2D<bool>(width, height);
        var bitIndex = 0;
        var arrayIndex = 0;
        for (var iRow = 0; iRow < height; iRow++)
        {
            for (var iCol = 0; iCol < width; iCol++)
            {
                if ((array[arrayIndex] & (1L << bitIndex)) != 0)
                {
                    map[iCol, iRow] = true;
                }
                if (++bitIndex == 64)
                {
                    bitIndex = 0;
                    arrayIndex++;
                }
            }
        }
    }
    #endregion
}
