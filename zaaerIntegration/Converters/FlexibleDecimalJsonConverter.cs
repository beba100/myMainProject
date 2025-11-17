using System.Text.Json;
using System.Text.Json.Serialization;

namespace zaaerIntegration.Converters
{
    /// <summary>
    /// Allows nullable decimals to be provided either as numbers or strings (e.g., "106.00").
    /// Empty strings and "null" become null when target type is nullable; for non-nullable, default 0.
    /// </summary>
    public class FlexibleDecimalJsonConverter : JsonConverter<decimal?>
    {
        public override decimal? Read(ref Utf8JsonReader reader, System.Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Number)
            {
                if (reader.TryGetDecimal(out var dec)) return dec;
                return null;
            }
            if (reader.TokenType == JsonTokenType.String)
            {
                var s = reader.GetString();
                if (string.IsNullOrWhiteSpace(s) || s == "\"\"" || s == "null") return null;
                if (decimal.TryParse(s, out var dec)) return dec;
                return null;
            }
            if (reader.TokenType == JsonTokenType.Null)
            {
                return null;
            }
            return null;
        }

        public override void Write(Utf8JsonWriter writer, decimal? value, JsonSerializerOptions options)
        {
            if (value.HasValue) writer.WriteNumberValue(value.Value);
            else writer.WriteNullValue();
        }
    }

    /// <summary>
    /// Allows non-nullable decimals to be provided either as numbers or strings (e.g., "106.00").
    /// For non-nullable decimals, defaults to 0 if value cannot be parsed.
    /// </summary>
    public class FlexibleNonNullableDecimalJsonConverter : JsonConverter<decimal>
    {
        public override decimal Read(ref Utf8JsonReader reader, System.Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Number)
            {
                if (reader.TryGetDecimal(out var dec)) return dec;
                return 0m;
            }
            if (reader.TokenType == JsonTokenType.String)
            {
                var s = reader.GetString();
                if (string.IsNullOrWhiteSpace(s) || s == "\"\"" || s == "null") return 0m;
                if (decimal.TryParse(s, out var dec)) return dec;
                return 0m;
            }
            if (reader.TokenType == JsonTokenType.Null)
            {
                return 0m;
            }
            return 0m;
        }

        public override void Write(Utf8JsonWriter writer, decimal value, JsonSerializerOptions options)
        {
            writer.WriteNumberValue(value);
        }
    }
}


