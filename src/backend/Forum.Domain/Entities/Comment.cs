namespace Forum.Domain.Entities;

public class Comment
{
    public int Id { get; set; }
    public int PostId { get; set; }
    public string AuthorId { get; set; } = "";
    public string Body { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
