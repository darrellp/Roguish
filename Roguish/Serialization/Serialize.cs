using Newtonsoft.Json;
using System.Diagnostics;
using System.Text;
using Roguish.ECS;
using Roguish.ECS.Components;
using EcsRx.Extensions;
using EcsRx.Infrastructure;
using SystemsRx.Infrastructure.Dependencies;
using Ninject;
using Roguish.Screens;
using System.Data;
using System.Reflection;

// ReSharper disable IdentifierTypo

namespace Roguish.Serialization;
internal static partial class Serialize
{
    private static readonly string ComponentNamespace = typeof(HealthComponent).Namespace!;


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
        DeserializeEcs(jsonEcs);
    }

    private static void DeserializeEcs(string json)
    {
        var reader = new JsonTextReader(new StringReader(json));
        reader.Read();
        Debug.Assert(reader.TokenType == JsonToken.StartArray);
        while (reader.Read())       // Start of object
        {

            if (reader.TokenType == JsonToken.EndArray)
                break;
            ReadEntity(reader);
        }
    }

    private static void ReadEntity(JsonReader reader)
    {
        int id;
        Debug.Assert(reader.TokenType == JsonToken.StartObject);
        reader.Read();
        Debug.Assert(reader.TokenType == JsonToken.PropertyName);
        Debug.Assert((string)reader.Value! == "Id");
        reader.Read();
        Debug.Assert(reader.TokenType == JsonToken.Integer);
        id = Convert.ToInt32(reader.Value!);
        reader.Read();
        Debug.Assert(reader.TokenType == JsonToken.PropertyName);
        Debug.Assert(reader.Value == "Components");
        reader.Read();
        Debug.Assert(reader.TokenType == JsonToken.StartArray);

        // Components
        while (true)
        {
            var cmp = DeserializeComponent(reader);
            if (cmp == null)
            {
                break;
            }
        }
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

    private static EcsComponent? DeserializeComponent(string json)
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
        return type == null ? null : (EcsComponent?)JsonConvert.DeserializeObject(json, type);
    }

    private static EcsComponent? DeserializeComponent(JsonReader reader)
    {
        JsonReader reader2 = reader;
        reader2.Read();
        //if (reader2.TokenType != JsonToken.StartObject)
        //{
        //    return null;
        //}

        return DeserializeComponent(reader2.Value as string);
        reader2.Read();
        Debug.Assert(reader2.TokenType == JsonToken.PropertyName);
        Debug.Assert(reader2.Value == "ComponentType");
        reader2.Read();
        Debug.Assert(reader2.TokenType == JsonToken.String);
        var typeName = ComponentNamespace + "." + reader2.Value;
        var type = Type.GetType(typeName);

        JsonSerializer serializer = new JsonSerializer();
        var method = typeof(JsonSerializer).GetMethod("Deserialize", [typeof(JsonReader)]);
        Type[] typeArgs = [type];
        MethodInfo genericMethod = method.MakeGenericMethod(typeArgs);
        return genericMethod.Invoke(serializer, [reader]) as EcsComponent;
    }
}
