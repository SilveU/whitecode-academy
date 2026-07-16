using System.Text.Json;
using System.Text.Json.Serialization;

namespace Application.Helper
{
    public static class CacheSerializer
    {
        private static readonly JsonSerializerOptions Options = new()
        {
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            WriteIndented = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        public static string Serialize<T>(T value)
            => JsonSerializer.Serialize(value, Options);

        public static T? Deserialize<T>(string json)
            => JsonSerializer.Deserialize<T>(json, Options);
    }
}