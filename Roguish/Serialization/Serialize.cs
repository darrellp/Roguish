using Newtonsoft.Json;
using System.Diagnostics;
using System.Text;
using Ninject;
using Roguish.ECS.Components;
using Roguish.Screens;

// ReSharper disable IdentifierTypo

namespace Roguish.Serialization;
internal static partial class Serialize
{
    private static readonly string ComponentNamespace = typeof(HealthComponent).Namespace!;
    private static DungeonSurface Dungeon = Kernel.Get<DungeonSurface>();

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
        var mpOldIdToNewId = new Dictionary<int, int>();
        CreateEntities(ecsInfo, mpOldIdToNewId);

        ReanimateHero(FindHero(ecsInfo), ecsInfo, mpOldIdToNewId);
    }

    private static void CreateEntities(List<EntityInfo> ecsInfo, Dictionary<int, int> mpOldIdToNewId)
    {
        // TODO: Actually create entities and map the old ids in ecsInfo to the new entity ids
        for (var i = 0; i < ecsInfo.Count; i++)
        {
            mpOldIdToNewId[i] = i;    
        }
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

        foreach (var cmp in heroInfo.Components)
        {
            
        }
    }

    private static void MassageComponents(EntityInfo entityInfo, Dictionary<int, int> mpOldIdToNewId)
    {
        foreach (var cmp in entityInfo.Components) 
        {
            switch (cmp)
            {
                case DisplayComponent displayComponent:
                    var posCmp = entityInfo.FindComponent<PositionComponent>();
                    if (posCmp != null)
                    {
                        displayComponent.ScEntity = Dungeon.GetPlayerScEntity(posCmp.Position.Value);
                    }
                    break;

                case EquippedComponent equippedComponent:
                    equippedComponent.RemapEquipment(mpOldIdToNewId);
                    break;
            }
        }
    }

    internal static int RemapId(int id, Dictionary<int, int> mpOldIdToNewId)
    {
        return mpOldIdToNewId.GetValueOrDefault(id, id);
    }

    private record EntityInfo(int Id, List<EcsComponent> Components)
    {
        public T? FindComponent<T>() where T : class
        {
            return Components.FirstOrDefault(c => c is T) as T;
        }
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
        reader.Read();  // Id:
        reader.Read();  // <Id value>
        var id = Convert.ToInt32(reader.Value!);
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

        return new EntityInfo(id, componentList);
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
        writer.WritePropertyName("Id");
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


#if NOTNOW

    private static void PlayerSerialization(JsonSerializerSettings settings)
    {
        var player = EcsRxApp.Player;

        var descCmp = player.GetComponent<DescriptionComponent>();
        var descJson = JsonConvert.SerializeObject(descCmp, settings);
        var descCmpD = DeserializeComponent(descJson);
        Debug.Assert(descCmpD != null, "Something didn't Deserialize correctly!");

        var lvlCmp = player.GetComponent<LevelItemComponent>();
        var lvlJson = JsonConvert.SerializeObject(lvlCmp, settings);
        var lvlCmpD = DeserializeComponent<LevelItemComponent>(lvlJson);
        Debug.Assert(lvlCmpD != null, "Something didn't Deserialize correctly!");

        var posCmp = player.GetComponent<PositionComponent>();
        var posJson = JsonConvert.SerializeObject(posCmp, settings);
        var posCmpD = DeserializeComponent<PositionComponent>(posJson);
        Debug.Assert(posCmpD != null, "Something didn't Deserialize correctly!");

        var ispcCmp = player.GetComponent<IsPlayerControlledComponent>();
        var ispcJson = JsonConvert.SerializeObject(ispcCmp, settings);
        var ispcCmpD = DeserializeComponent<IsPlayerControlledComponent>(ispcJson);
        Debug.Assert(ispcCmpD != null, "Something didn't Deserialize correctly!");

        var entTCmp = player.GetComponent<EntityTypeComponent>();
        var entTJson = JsonConvert.SerializeObject(entTCmp, settings);
        var entTCmpD = DeserializeComponent<EntityTypeComponent>(entTJson);
        Debug.Assert(entTCmpD != null, "Something didn't Deserialize correctly!");

        var hlthCmp = player.GetComponent<HealthComponent>();
        var hlthJson = JsonConvert.SerializeObject(hlthCmp, settings);
        var hlthCmpD = DeserializeComponent<HealthComponent>(hlthJson);
        Debug.Assert(hlthCmpD != null, "Something didn't Deserialize correctly!");

        // We can actually create the entity based on the EntityTypeComponent and PositionComponent above so no need to
        // worry about this too much...
        var dispCmp = player.GetComponent<DisplayComponent>();
        var dispJson = JsonConvert.SerializeObject(dispCmp, settings);
        var dispCmpD = DeserializeComponent<DisplayComponent>(dispJson);
        Debug.Assert(dispCmpD != null, "Something didn't Deserialize correctly!");

        var equpCmp = player.GetComponent<EquippedComponent>();
        var equpJson = JsonConvert.SerializeObject(equpCmp, settings);
        var equpCmpD = DeserializeComponent<EquippedComponent>(equpJson);
        Debug.Assert(equpCmpD != null, "Something didn't Deserialize correctly!");

        var taskCmp = player.GetComponent<TaskComponent>();
        var taskJson = JsonConvert.SerializeObject(taskCmp, settings);
        var taskCmpD = DeserializeComponent<TaskComponent>(taskJson);
        Debug.Assert(taskCmpD != null, "Something didn't Deserialize correctly!");
    }

    private static T? DeserializeComponent<T>(string json)
    {
        var reader = new JsonTextReader(new StringReader(json));
        reader.Read();
        Debug.Assert(reader.TokenType == JsonToken.StartObject);
        reader.Read();
        Debug.Assert(reader.TokenType == JsonToken.PropertyName);
        Debug.Assert(reader.Value == "ComponentType");
        reader.Read();
        Debug.Assert(reader.TokenType == JsonToken.String);
        var typeName = ComponentNamespace + "." + reader.Value;
        var type = Type.GetType(typeName);
        return type == null ? default(T) : JsonConvert.DeserializeObject<T>(json);
    }
#endif
}
