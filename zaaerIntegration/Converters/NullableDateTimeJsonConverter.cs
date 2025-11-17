using System.Text.Json;
using System.Text.Json.Serialization;

namespace zaaerIntegration.Converters
{
    /// <summary>
    /// Custom JSON converter that treats empty strings as null for nullable DateTime values
    /// </summary>
    public class NullableDateTimeJsonConverter : JsonConverter<DateTime?>
    {
        public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
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
                // Try to parse as DateTime
                if (DateTime.TryParse(stringValue, out var dateTimeValue))
                {
                    return dateTimeValue;
                }
                // If parsing fails but it's an empty-like value, return null
                return null;
            }

            // Handle null token
            if (reader.TokenType == JsonTokenType.Null)
            {
                return null;
            }

            // For any other token type, try to deserialize using default behavior
            // This handles ISO 8601 date strings that System.Text.Json can parse directly
            try
            {
                return JsonSerializer.Deserialize<DateTime?>(ref reader, options);
            }
            catch
            {
                // If deserialization fails, return null (graceful handling)
                return null;
            }
        }

        public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
        {
            if (value.HasValue)
            {
                writer.WriteStringValue(value.Value);
            }
            else
            {
                writer.WriteNullValue();
            }
        }
    }
}

