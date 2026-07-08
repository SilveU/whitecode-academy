namespace Application.DTOs.Core
{
    public record DepartmentResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string? ImageUrl { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}
