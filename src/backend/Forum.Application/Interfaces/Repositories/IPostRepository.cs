using Forum.Application.Dtos;
using Forum.Application.Dtos.Posts;
using Forum.Application.Enums;
using Forum.Domain.Entities;

namespace Forum.Application.Interfaces.Repositories;

public interface IPostRepository
{
    Task<Page<PostResponseDto>> GetPosts(PostQueryParametersDto query, SortByOptions sort, string? viewer, CancellationToken ct);
    Task<PostResponseDto> GetPost(int id, string? viewer, CancellationToken ct);
    Task<Post> RequirePost(int id, CancellationToken ct);
    Task<List<AuthorResponseDto>> GetAuthors(CancellationToken ct);
    Task<int> Add(Post post, CancellationToken ct);
    Task AddLike(Like like, CancellationToken ct);
    Task AddTag(ModerationTag tag, CancellationToken ct);
}