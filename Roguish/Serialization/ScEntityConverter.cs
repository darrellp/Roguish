using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Roguish.Serialization;
internal class ScEntityConverter : JsonConverter<ScEntity>
{
    public override void WriteJson(JsonWriter writer, ScEntity? value, JsonSerializer serializer)
    {
        var singleCellGlyph = value.AppearanceSingle;
        var position = value.AbsolutePosition;
        var fg = singleCellGlyph.Appearance.Foreground;
        var glyph = singleCellGlyph.Appearance.Glyph;
        var zOrder = value.ZIndex;
        writer.WriteStartObject();
        writer.WritePropertyName("Foreground");
        writer.WriteValue(fg.PackedValue);
        writer.WritePropertyName("Glyph");
        writer.WriteValue(glyph);
        writer.WritePropertyName("ZOrder");
        writer.WriteValue(zOrder);
        writer.WritePropertyName("Position");
        serializer.Serialize(writer, position);
        writer.WriteEndObject();
    }

    public override ScEntity? ReadJson(JsonReader reader, Type objectType, ScEntity? existingValue, bool hasExistingValue,
        JsonSerializer serializer)
    {
        return null;
    }
}
