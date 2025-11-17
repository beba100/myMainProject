using System.Text.Json;
using System.Text.Json.Serialization;

namespace zaaerIntegration.Converters
{
    /// <summary>
    /// Allows booleans to be provided either as booleans (true/false) or integers (0/1).
    /// This is useful for Zaaer integration which sends status as integer (0/1) instead of boolean.
    /// </summary>
    public class FlexibleBooleanJsonConverter : JsonConverter<bool>
    {
        public override bool Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.True)
            {
                return true;
            }
            if (reader.TokenType == JsonTokenType.False)
            {
                return false;
            }
            if (reader.TokenType == JsonTokenType.Number)
            {
                // Handle integer values: 0 = false, any other number = true
                var intValue = reader.GetInt32();
                return intValue != 0;
            }
            if (reader.TokenType == JsonTokenType.String)
            {
                var stringValue = reader.GetString();
                if (string.IsNullOrWhiteSpace(stringValue))
                {
                    return false;
                }
                // Try parsing as boolean
                if (bool.TryParse(stringValue, out var boolValue))
                {
                    return boolValue;
                }
                // Try parsing as integer
                if (int.TryParse(stringValue, out var intValue))
                {
                    return intValue != 0;
                }
                return false;
            }
            if (reader.TokenType == JsonTokenType.Null)
            {
                return false; // Default to false for null
            }
            return false;
        }

        public override void Write(Utf8JsonWriter writer, bool value, JsonSerializerOptions options)
        {
            writer.WriteBooleanValue(value);
        }
    }

    /// <summary>
    /// Allows nullable booleans to be provided either as booleans (true/false) or integers (0/1).
    /// </summary>
    public class FlexibleNullableBooleanJsonConverter : JsonConverter<bool?>
    {
        public override bool? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.True)
            {
                return true;
            }
            if (reader.TokenType == JsonTokenType.False)
            {
                return false;
            }
            if (reader.TokenType == JsonTokenType.Number)
            {
                // Handle integer values: 0 = false, any other number = true
                var intValue = reader.GetInt32();
                return intValue != 0;
            }
            if (reader.TokenType == JsonTokenType.String)
            {
                var stringValue = reader.GetString();
                if (string.IsNullOrWhiteSpace(stringValue))
                {
                    return null;
                }
                // Try parsing as boolean
                if (bool.TryParse(stringValue, out var boolValue))
                {
                    return boolValue;
                }
                // Try parsing as integer
                if (int.TryParse(stringValue, out var intValue))
                {
                    return intValue != 0;
                }
                return null;
            }
            if (reader.TokenType == JsonTokenType.Null)
            {
                return null;
            }
            return null;
        }

        public override void Write(Utf8JsonWriter writer, bool? value, JsonSerializerOptions options)
        {
            if (value.HasValue)
            {
                writer.WriteBooleanValue(value.Value);
            }
            else
            {
                writer.WriteNullValue();
            }
        }
    }
}

