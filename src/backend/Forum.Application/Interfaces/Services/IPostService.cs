using Forum.Application.Dtos;
using Forum.Application.Dtos.Posts;

namespace Forum.Application.Interfaces.Services;

public interface IPostService
{
    Task<Page<PostResponseDto>> GetPosts(PostQueryParametersDto query, string? viewer, CancellationToken ct);
    Task<PostResponseDto> GetPost(int id, string? viewer, CancellationToken ct);
    Task<List<AuthorResponseDto>> GetAuthors(CancellationToken ct);
    Task<int> CreatePost(string authorId, CreatePostDto request, CancellationToken ct);
    Task LikePost(int postId, string userId, CancellationToken ct);
    Task TagPost(int postId, string moderatorId, TagPostDto? request, CancellationToken ct);
}