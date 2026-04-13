using System.Text.Json;
using System.Text.Json.Serialization;
using IAT.Core.Domain;

namespace IAT.Core.Services;

/// <summary>
/// Serializes and deserializes Stimulus objects, handling polymorphism by including a "StimulusType" discriminator in the JSON. This allows the correct derived type 
/// (e.g., ImageStimulus, TextStimulus) to be instantiated during deserialization.
/// </summary>
public class JsonPolymorphicStimulusConverter : JsonConverter<Stimulus>
{
    /// <summary>
    /// Reads and deserializes a JSON representation of a Stimulus object based on its 'StimulusType' property.
    /// </summary>
    /// <remarks>The method supports deserialization of 'ImageStimulus' and 'TextStimulus' types. The JSON
    /// input must include a 'StimulusType' property to indicate the specific type to instantiate.</remarks>
    /// <param name="reader">A reference to the Utf8JsonReader positioned at the value to read. The reader is advanced past the consumed JSON
    /// value.</param>
    /// <param name="typeToConvert">The type of object to convert. This parameter is not used and can be ignored.</param>
    /// <param name="options">Options to control the behavior of the JSON deserialization.</param>
    /// <returns>A Stimulus instance deserialized from the JSON input. The specific derived type is determined by the
    /// 'StimulusType' property in the JSON.</returns>
    /// <exception cref="JsonException">Thrown if the JSON does not contain a 'StimulusType' property, if the 'StimulusType' value is unrecognized, or
    /// if deserialization fails.</exception>
    public override Stimulus Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;

        if (!root.TryGetProperty("StimulusType", out var typeProp))
        {
            throw new JsonException("Missing 'StimulusType' property for Stimulus deserialization.");
        }

        if (typeProp.GetString() == "Image")
        {
            return JsonSerializer.Deserialize<ImageStimulus>(root.GetRawText(), options) ?? throw new JsonException("Failed to deserialize Stimulus.");
        }
        else if (typeProp.GetString() == "Text")
        {
            return JsonSerializer.Deserialize<TextStimulus>(root.GetRawText(), options) ?? throw new JsonException("Failed to deserialize Stimulus.");
        }
        else
        {
            throw new JsonException($"Unknown StimulusType: {typeProp.GetString()}");
        }
    }

    /// <summary>
    /// Writes a JSON representation of the specified Stimulus object, including a type discriminator property.
    /// </summary>
    /// <remarks>A discriminator property named "StimulusType" is included in the output to indicate the
    /// specific derived type of the Stimulus object. Only recognized Stimulus subtypes can be serialized; unrecognized
    /// types will result in an exception.</remarks>
    /// <param name="writer">The Utf8JsonWriter to which the JSON will be written.</param>
    /// <param name="value">The Stimulus object to serialize. Must not be null and must be a recognized derived type.</param>
    /// <param name="options">The options to use when serializing the object.</param>
    /// <exception cref="JsonException">Thrown if the type of the value parameter is not a recognized Stimulus subtype.</exception>
    public override void Write(Utf8JsonWriter writer, Stimulus value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        // Add the discriminator
        var stimulusType = value switch
        {
            ImageStimulus => "Image",
            TextStimulus => "Text",
            // Add other types
            _ => throw new JsonException($"Unknown Stimulus type: {value.GetType()}")
        };
        writer.WriteString("StimulusType", stimulusType);

        // Serialize the properties of the base class and derived class
        var json = JsonSerializer.Serialize(value, value.GetType(), options);
        using var doc = JsonDocument.Parse(json);
        foreach (var prop in doc.RootElement.EnumerateObject())
        {
            prop.WriteTo(writer);
        }

        writer.WriteEndObject();
    }
}