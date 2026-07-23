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
    /// <summary>
    /// Reads and converts the JSON representation of a KeyedDirection object.
    /// </summary>
    /// <param name="reader">The reader to read the JSON from.</param>
    /// <param name="typeToConvert">The type to convert.</param>
    /// <param name="options">The serializer options.</param>
    /// <returns>The deserialized KeyedDirection object.</returns>
    /// <exception cref="JsonException"></exception>
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

    /// <summary>
    /// Writes a KeyedDirection object as JSON.
    /// </summary>
    /// <param name="writer">The writer to write the JSON to.</param>
    /// <param name="value">The KeyedDirection value to write.</param>
    /// <param name="options">The serializer options.</param>
    public override void Write(Utf8JsonWriter writer, KeyedDirection value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteString("Name", value.Name);
        writer.WriteEndObject();
    }
}