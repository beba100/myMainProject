using System.Text.Json;
using System.Text.Json.Serialization;

namespace zaaerIntegration.Converters
{
    /// <summary>
    /// Custom JSON converter that treats empty strings as null for nullable int values
    /// </summary>
    public class NullableIntJsonConverter : JsonConverter<int?>
    {
        public override int? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            // Handle string values (including empty strings)
            if (reader.TokenType == JsonTokenType.String)
            {
                var stringValue = reader.GetString();
                // Treat empty strings, null, or whitespace as null
                if (string.IsNullOrWhiteSpace(stringValue) || stringValue == "\"\"" || stringValue == "null")
                {
                    return null;
                }
                // Try to parse as integer
                if (int.TryParse(stringValue, out var intValue))
                {
                    return intValue;
                }
                // If parsing fails but it's an empty-like value, return null
                return null;
            }

            // Handle null token
            if (reader.TokenType == JsonTokenType.Null)
            {
                return null;
            }

            // Handle number token (valid integer)
            if (reader.TokenType == JsonTokenType.Number)
            {
                return reader.GetInt32();
            }

            // For any other token type, return null (graceful handling)
            return null;
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

