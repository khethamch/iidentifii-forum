namespace Forum.Domain.Entities;

public class Post
{
    public int Id { get; set; }
    public string AuthorId { get; set; } = "";
    public string Title { get; set; } = "";
    public string Body { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public List<Comment> Comments { get; set; } = [];
    public List<Like> Likes { get; set; } = [];
    public List<ModerationTag> Tags { get; set; } = [];
}
