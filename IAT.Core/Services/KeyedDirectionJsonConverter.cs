using System.Text.Json;
using System.Text.Json.Serialization;
using IAT.Core.Enumerations;

namespace IAT.Core.Services;

/// <summary>
/// Handles serialization and deserialization of the closed KeyedDirection hierarchy
/// (Left / Right / None) using the "Name" property as the discriminator.
/// </summary>
public sealed class KeyedDirectionJsonConverter : JsonConverter<KeyedDirection>
{
    public override KeyedDirection Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);

        if (!doc.RootElement.TryGetProperty("Name", out var nameProp))
            throw new JsonException("KeyedDirection JSON is missing the required 'Name' property.");

        var name = nameProp.GetString();

        return name switch
        {
            "Left" => KeyedDirection.Left,
            "Right" => KeyedDirection.Right,
            "None" => KeyedDirection.None,
            _ => throw new JsonException($"Unknown KeyedDirection value: '{name}'")
        };
    }

    public override void Write(Utf8JsonWriter writer, KeyedDirection value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteString("Name", value.Name);
        writer.WriteEndObject();
    }
}