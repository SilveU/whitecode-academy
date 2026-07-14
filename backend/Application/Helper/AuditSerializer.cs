using System.Text.Json;
using System.Text.Json.Serialization;

namespace Application.Helper
{
    /// <summary>
    /// Serializes entities to JSON for audit log values.
    /// Uses ReferenceHandler.IgnoreCycles to prevent infinite loops
    /// caused by circular navigation properties (e.g. Course → Instructor → Courses...).
    /// </summary>
    public static class AuditSerializer
    {
        private static readonly JsonSerializerOptions Options = new()
        {
            ReferenceHandler    = ReferenceHandler.IgnoreCycles,
            WriteIndented       = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        public static string Serialize<T>(T value) where T : class
            => JsonSerializer.Serialize(value, Options);
    }
}
