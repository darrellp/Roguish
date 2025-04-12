using Newtonsoft.Json;
using System.Diagnostics;
using System.Text;
using Ninject;
using Roguish.ECS;
using Roguish.ECS.Components;
using Roguish.Map_Generation;
using Roguish.Screens;
using EcsRx.Extensions;
using GoRogue.Random;
using Roguish.ECS.Systems;
using SadConsole.SerializedTypes;
using SadRogue.Primitives.GridViews;
using ShaiRandom.Generators;

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

    internal static void SaveGame()
    {
        var sb = new StringBuilder();
        var sw = new StringWriter(sb);

        using JsonWriter writer = new JsonTextWriter(sw);
        SerializeEcs(writer);
        var jsonEcs = sb.ToString();
        var ecsInfo = DeserializeEcs(jsonEcs);
        DeserializeEcs(ecsInfo);
    }

    internal static void TestPacking()
    {
        var rng = GlobalRandom.DefaultRNG;
        var test = new ArrayView2D<bool>(10, 10);
        for (var iRow = 0; iRow < test.Height; iRow++)
        {
            for (var iCol = 0; iCol < test.Width; iCol++)
            {
                test[iCol, iRow] = rng.NextInt(2) == 1;
            }
        }
        var packed = PackBoolArray(test);
        var unpacked = UnpackBoolArray(packed, test.Width, test.Height);

        for (var iRow = 0; iRow < test.Height; iRow++)
        {
            for (var iCol = 0; iCol < test.Width; iCol++)
            {
                if (test[iRow, iCol] != unpacked[iRow, iCol])
                {
                    Debugger.Break();
                }
            }
        }
    }

    #region Deserialization
    private static void DeserializeEcs(List<EntityInfo> ecsInfo)
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

        Dungeon.SetVisibilities();
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
    #endregion

    #region Serialization

    private static void SerializeMaps(JsonWriter writer)
    {
        var packed = PackBoolArray(MapGenerator.RevealMap);
        writer.WritePropertyName("RevealMap");
        var json = JsonConvert.SerializeObject(packed);
        writer.WriteValue(json);
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

    private static ISettableGridView<bool> UnpackBoolArray(long[] array, int width, int height)
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
                    ret[iCol, iRow] = true;
                }
                if (++bitIndex == 64)
                {
                    bitIndex = 0;
                    arrayIndex++;
                }
            }
        }
        return ret;
    }
    #endregion
}
