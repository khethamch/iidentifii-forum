namespace Forum.Domain.Entities;

public class ModerationTag
{
    public int Id { get; set; }
    public int PostId { get; set; }
    public string ModeratorId { get; set; } = "";
    public string Label { get; set; } = "Misleading or false information";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}