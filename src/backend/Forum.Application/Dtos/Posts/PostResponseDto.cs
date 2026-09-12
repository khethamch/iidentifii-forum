namespace Forum.Application.Dtos.Posts;

public record PostResponseDto(int Id, string AuthorId, string Author, string Title, string Body, DateTime CreatedAt,
    int LikeCount, int CommentCount, bool LikedByMe, List<ModerationTagResponseDto> Tags);