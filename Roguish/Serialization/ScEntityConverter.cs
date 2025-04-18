using System.Diagnostics;
using Newtonsoft.Json;
using Roguish.Screens;

namespace Roguish.Serialization;
internal class ScEntityConverter(DungeonSurface dungeon) : JsonConverter<ScEntity>
{
    public override void WriteJson(JsonWriter writer, ScEntity? value, JsonSerializer serializer)
    {
        Debug.Assert(value != null, nameof(value) + " != null");
        var singleCellGlyph = value.AppearanceSingle;
        Debug.Assert(singleCellGlyph != null, nameof(singleCellGlyph) + " != null");
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

    public override ScEntity ReadJson(JsonReader reader, Type objectType, ScEntity? existingValue, bool hasExistingValue,
        JsonSerializer serializer)
    {
        reader.Read();      // PropertyName "Foreground"
        var packed = (uint)(reader.ReadAsDouble() ?? 0);
        var fg = new Color(packed);
        reader.Read();      // PropertyName "Glyph"
        var glyph = reader.ReadAsInt32() ?? 219;
        reader.Read();      // PropertyName "ZOrder"
        var zOrder = reader.ReadAsInt32() ?? 0;
        reader.Read();      // PropertyName "Position"
        reader.Read();      // ObjectStart
        reader.Read();      // PropertyName "X"
        var x = reader.ReadAsInt32() ?? 0;
        reader.Read();      // PropertyName "Y"
        var y = reader.ReadAsInt32() ?? 0;
        var position = new Point(x, y);
        reader.Read();      // ObjectEnd
        reader.Read();      // ObjectEnd
        return dungeon.CreateScEntity(fg, position, glyph, zOrder);
    }
}
