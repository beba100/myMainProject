using System.Text.Json;
using System.Text.Json.Serialization;

namespace zaaerIntegration.Converters
{
    /// <summary>
    /// Custom JSON converter that allows empty strings to be converted to null for nullable integers
    /// </summary>
    public class NullableIntConverter : JsonConverter<int?>
    {
        public override int? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                return null;
            }

            if (reader.TokenType == JsonTokenType.String)
            {
                var stringValue = reader.GetString();
                if (string.IsNullOrEmpty(stringValue))
                {
                    return null;
                }
                
                if (int.TryParse(stringValue, out var intValue))
                {
                    return intValue;
                }
                
                return null;
            }

            if (reader.TokenType == JsonTokenType.Number)
            {
                return reader.GetInt32();
            }

            throw new JsonException($"Unable to convert {reader.TokenType} to int?");
        }

        public override void Write(Utf8JsonWriter writer, int? value, JsonSerializerOptions options)
        {
            if (value.HasValue)
            {
                writer.WriteNumberValue(value.Value);
            }
            else
            {
                writer.WriteNullValue();
            }
        }
    }
}
