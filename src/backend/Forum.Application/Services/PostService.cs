using Forum.Application.Dtos;
using Forum.Application.Dtos.Posts;
using Forum.Application.Enums;
using Forum.Domain.Entities;
using Forum.Application.Interfaces.Repositories;
using Forum.Application.Interfaces.Services;
using Forum.Domain.Exceptions;
using Forum.Domain.Rules;

namespace Forum.Application.Services;

public sealed class PostService(IPostRepository posts) : IPostService
{
    public Task<Page<PostResponseDto>> GetPosts(PostQueryParametersDto query, string? viewer, CancellationToken ct)
    {
        ForumRules.ValidatePage(query.Page, query.PageSize);
        if (query.From > query.To) throw new ForumException("From must be before To.");
        if (query.Tag is not (null or "" or "flagged" or "unflagged"))
            throw new ForumException("Unknown moderation filter.");

        var sort = query.Sort switch
        {
            "newest" => SortByOptions.DateDescending,
            "oldest" => SortByOptions.DateAscending,
            "likes" => SortByOptions.Likes,
            _ => throw new ForumException("Sort must be newest, oldest or likes.")
        };
        return posts.GetPosts(query, sort, viewer, ct);
    }

    public Task<PostResponseDto> GetPost(int id, string? viewer, CancellationToken ct) =>
        posts.GetPost(id, viewer, ct);

    public Task<List<AuthorResponseDto>> GetAuthors(CancellationToken ct) => posts.GetAuthors(ct);

    public Task<int> CreatePost(string authorId, CreatePostDto request, CancellationToken ct) =>
        posts.Add(new Post
        {
            AuthorId = authorId,
            Title = ForumRules.RequiredText(request.Title, 180),
            Body = ForumRules.RequiredText(request.Body, 10000)
        }, ct);

    public async Task LikePost(int postId, string userId, CancellationToken ct)
    {
        var post = await posts.RequirePost(postId, ct);

        ForumRules.EnsureCanLike(post.AuthorId, userId);
        await posts.AddLike(new Like { PostId = postId, UserId = userId }, ct);
    }

    public async Task TagPost(int postId, string moderatorId, TagPostDto? request, CancellationToken ct)
    {
        await posts.RequirePost(postId, ct);
        if (request is not null && request.Label != "Misleading or false information")
            throw new ForumException("The only supported tag is Misleading or false information.");

        await posts.AddTag(new ModerationTag { PostId = postId, ModeratorId = moderatorId }, ct);
    }
}
