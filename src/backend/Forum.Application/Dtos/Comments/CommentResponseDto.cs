namespace Forum.Application.Dtos.Comments;

public record CommentResponseDto(int Id, string Author, string Body, DateTime CreatedAt);
