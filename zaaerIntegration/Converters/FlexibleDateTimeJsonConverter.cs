using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace zaaerIntegration.Converters
{
    /// <summary>
    /// Parses ISO-8601 or space-separated date/time strings into non-nullable DateTime values.
    /// Accepts payloads such as "2025-11-08T11:00:00" and "2025-11-08 11:00:00".
    /// </summary>
    public class FlexibleDateTimeJsonConverter : JsonConverter<DateTime>
    {
        private static readonly string[] AcceptedFormats =
        {
            "yyyy-MM-dd'T'HH:mm:ss",
            "yyyy-MM-dd'T'HH:mm:ss.fff",
            "yyyy-MM-dd HH:mm:ss",
            "yyyy-MM-dd HH:mm:ss.fff",
            "yyyy/MM/dd HH:mm:ss",
            "yyyy/MM/dd'T'HH:mm:ss",
            "MM/dd/yyyy HH:mm:ss",
            "MM/dd/yyyy'T'HH:mm:ss"
        };

        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                var stringValue = reader.GetString();
                if (string.IsNullOrWhiteSpace(stringValue))
                {
                    throw new JsonException("Date value cannot be empty.");
                }

                if (DateTime.TryParseExact(stringValue, AcceptedFormats, CultureInfo.InvariantCulture,
                        DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var exactResult))
                {
                    return exactResult;
                }

                if (DateTime.TryParse(stringValue, CultureInfo.InvariantCulture,
                        DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var invariantResult))
                {
                    return invariantResult;
                }

                if (DateTime.TryParse(stringValue, CultureInfo.CurrentCulture, DateTimeStyles.None, out var cultureResult))
                {
                    return cultureResult;
                }

                throw new JsonException($"Invalid date/time value: {stringValue}");
            }

            if (reader.TokenType == JsonTokenType.Number)
            {
                return reader.GetDateTime();
            }

            return reader.GetDateTime();
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToUniversalTime().ToString("O"));
        }
    }
}
