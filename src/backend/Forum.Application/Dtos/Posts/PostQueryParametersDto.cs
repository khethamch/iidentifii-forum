namespace Forum.Application.Dtos.Posts;

public sealed class PostQueryParametersDto
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? Author { get; set; }
    public string? Tag { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    // Keep the published query values: newest, oldest, likes.
    public string Sort { get; set; } = "newest";
}