using Domain.Common;

namespace Domain.Entites.System
{
    public class Idempotency : BaseEntity
    {
        public string IdempotencyKey { get; set; } = null!;
        public string? UserId { get; set; }
        public string Path { get; set; } = null!;
        public string ResponseBody { get; set; } = null!;
        public int StatusCode { get; set; }
        public string ContentType { get; set; } = null!;
        public string HttpMethod { get; set; } = null!;
        public DateTimeOffset ExpiresAt { get; set; }
    }
}