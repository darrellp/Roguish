using System.Diagnostics;
using GoRogue;
using Newtonsoft.Json;
using SystemsRx.ReactiveData;

namespace Roguish.Serialization;
//internal class ReactiveConverter<T> : JsonConverter<ReactiveProperty<T>>
//{
//    public override void WriteJson(JsonWriter writer, ReactiveProperty<T>? value, JsonSerializer serializer)
//    {
//        if (value == null)
//        {
//            writer.WriteNull();
//            return;
//        }

//        var valueJson = value.Value.ToString();
//        //writer.WriteStartObject();
//        //writer.WritePropertyName("Value");
//        //writer.WriteValue(valueJson);
//        //writer.WriteEnd();
//        writer.WriteValue(valueJson);
//    }

//    public override ReactiveProperty<T>? ReadJson(JsonReader reader, Type objectType, ReactiveProperty<T>? existingValue, bool hasExistingValue,
//        JsonSerializer serializer)
//    {
//        Debug.Assert(reader != null, nameof(reader) + " != null");
//        var value = (T)reader.Value!;
//        return new ReactiveProperty<T>(value);
//    }
//}

public class ReactiveConverter<T> : JsonConverter<ReactiveProperty<T>>
{
    public override void WriteJson(JsonWriter writer, ReactiveProperty<T>? value, JsonSerializer serializer)
    {
        if (value == null)
        {
            writer.WriteNull();
        }
        else
        {
            serializer.Serialize(writer, value.Value);
        }
    }

    public override ReactiveProperty<T>? ReadJson(JsonReader reader, Type objectType, ReactiveProperty<T>? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        T? value = serializer.Deserialize<T>(reader);
        return value == null ? null : new ReactiveProperty<T>(value);
    }
}

