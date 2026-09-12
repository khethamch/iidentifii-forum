using Forum.Application.Dtos;
using Forum.Application.Dtos.Comments;
using Forum.Domain.Entities;
using Forum.Application.Interfaces.Repositories;
using Forum.Application.Interfaces.Services;
using Forum.Domain.Rules;

namespace Forum.Application.Services;

public sealed class CommentService(ICommentRepository comments, IPostRepository posts) : ICommentService
{
    public async Task<Page<CommentResponseDto>> GetComments(int postId, int page, int pageSize, CancellationToken ct)
    {
        ForumRules.ValidatePage(page, pageSize);

        await posts.RequirePost(postId, ct);
        return await comments.GetComments(postId, page, pageSize, ct);
    }

    public async Task<int> CreateComment(int postId, string authorId, CreateCommentDto request, CancellationToken ct)
    {
        var body = ForumRules.RequiredText(request.Body, 3000);
        
        await posts.RequirePost(postId, ct);
        return await comments.Add(new Comment { PostId = postId, AuthorId = authorId, Body = body }, ct);
    }
}
