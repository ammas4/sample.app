using System.Text.Json;
using System.Text.Json.Serialization;

namespace AppSample.Domain.Helpers;

/// <summary>
/// Используется для сериализации в формате, похожем на Dictionary,
/// но с возможностью использовать повторяющиеся ключи
/// </summary>
public class KeyValuePairJsonConverter : JsonConverter<KeyValuePair<string, string>[]>
{
    public override KeyValuePair<string, string>[]? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }

    public override void Write(Utf8JsonWriter writer, KeyValuePair<string, string>[] value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        foreach (var item in value)
        {
            writer.WriteStartObject();
            writer.WritePropertyName(item.Key.ToString());
            writer.WriteStringValue(item.Value.ToString());
            writer.WriteEndObject();
        }
        writer.WriteEndArray();
    }
}