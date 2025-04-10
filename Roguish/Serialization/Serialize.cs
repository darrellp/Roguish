using Newtonsoft.Json;
using System.Diagnostics;
using Roguish.ECS;
using Roguish.ECS.Components;
using EcsRx.Extensions;
using System.Dynamic;
using System.Runtime.Serialization;

// ReSharper disable IdentifierTypo

namespace Roguish.Serialization;
internal static partial class Serialize
{
    static string ComponentNamespace = typeof(HealthComponent).Namespace!;

    internal static void SaveGame()
    {
        JsonSerializerSettings settings = new JsonSerializerSettings
        {
            ContractResolver = new CustomResolver("MyTypeName"),
            Formatting = Formatting.Indented,
            Converters = new List<JsonConverter>
            {
                new ReactiveConverter<Point>(),
                new ReactiveConverter<Int64>(),
            }
        };

        PlayerSerialization(settings);
    }

    private static void PlayerSerialization(JsonSerializerSettings settings)
    {
        var player = EcsRxApp.Player;

        var descCmp = player.GetComponent<DescriptionComponent>();
        var descJson = JsonConvert.SerializeObject(descCmp, settings);
        var descCmpD = DeserializeComponent<DescriptionComponent>(descJson);
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
        dynamic? expando = JsonConvert.DeserializeObject<ExpandoObject>(json);
        if (expando == null)
        {
            throw new SerializationException("Invalid Json in DeserializeComponent");
        }
        string typeName = ComponentNamespace + "." + expando.ComponentType;
        var type = Type.GetType(typeName);
        return type == null ? default(T) : JsonConvert.DeserializeObject<T>(json);
    }
}
