using Forum.Application.Dtos;
using Forum.Application.Dtos.Comments;
using Forum.Domain.Entities;

namespace Forum.Application.Interfaces.Repositories;

public interface ICommentRepository
{
    Task<Page<CommentResponseDto>> GetComments(int postId, int page, int pageSize, CancellationToken ct);
    Task<int> Add(Comment comment, CancellationToken ct);
}