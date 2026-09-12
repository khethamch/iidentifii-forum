using Forum.Application.Dtos;
using Forum.Application.Dtos.Comments;

namespace Forum.Application.Interfaces.Services;

public interface ICommentService
{
    Task<Page<CommentResponseDto>> GetComments(int postId, int page, int pageSize, CancellationToken ct);
    Task<int> CreateComment(int postId, string authorId, CreateCommentDto request, CancellationToken ct);
}