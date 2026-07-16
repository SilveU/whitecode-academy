namespace Application.Common
{
    public record CachedHttpResponse
    {
        public int StatusCode { get; set; }
        public string ContentType { get; set; } = null!;
        public string Body { get; set; } = null!;
    }
}