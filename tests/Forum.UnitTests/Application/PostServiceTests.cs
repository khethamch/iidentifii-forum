using Forum.Application.Dtos;
using Forum.Application.Dtos.Posts;
using Forum.Application.Enums;
using Forum.Application.Interfaces.Repositories;
using Forum.Application.Services;
using Forum.Domain.Entities;
using Forum.Domain.Exceptions;

namespace Forum.UnitTests;

public class PostServiceTests
{
    [Fact]
    public async Task GivenOwnPost_WhenLiking_ThenRejectBeforeWriting()
    {
        var repository = new RecordingPosts();
        var service = new PostService(repository);

        await Assert.ThrowsAsync<ForumException>(() => service.LikePost(7, "author", default));

        Assert.Null(repository.SavedLike);
    }

    [Fact]
    public async Task GivenAnotherUsersPost_WhenLiking_ThenPersistTheUsersLike()
    {
        var repository = new RecordingPosts();
        var service = new PostService(repository);

        await service.LikePost(7, "reader", default);

        Assert.NotNull(repository.SavedLike);
        Assert.Equal(7, repository.SavedLike.PostId);
        Assert.Equal("reader", repository.SavedLike.UserId);
    }

    [Fact]
    public async Task GivenPaddedContent_WhenCreating_ThenPersistTrimmedPostForAuthor()
    {
        var repository = new RecordingPosts();
        var service = new PostService(repository);

        await service.CreatePost("author", new CreatePostDto(" Title ", " Body "), default);

        Assert.NotNull(repository.SavedPost);
        Assert.Equal("Title", repository.SavedPost.Title);
        Assert.Equal("Body", repository.SavedPost.Body);
        Assert.Equal("author", repository.SavedPost.AuthorId);
    }

    [Fact]
    public async Task GivenInvalidSort_WhenBrowsing_ThenRejectBeforeDatabaseQuery()
    {
        var service = new PostService(new RecordingPosts());

        await Assert.ThrowsAsync<ForumException>(() =>
            service.GetPosts(new PostQueryParametersDto { Sort = "unsupported" }, null, default));
    }

    // Unsupported operations throw so an unexpected repository call cannot silently pass a unit test.
    private sealed class RecordingPosts : IPostRepository
    {
        public Post? SavedPost { get; private set; }
        public Like? SavedLike { get; private set; }
        public Task<Post> RequirePost(int id, CancellationToken ct) => Task.FromResult(new Post { Id = id, AuthorId = "author" });
        public Task<int> Add(Post post, CancellationToken ct) { SavedPost = post; return Task.FromResult(7); }
        public Task AddLike(Like like, CancellationToken ct) { SavedLike = like; return Task.CompletedTask; }
        public Task AddTag(ModerationTag tag, CancellationToken ct) => throw new NotSupportedException();
        public Task<Page<PostResponseDto>> GetPosts(PostQueryParametersDto query, SortByOptions sort, string? viewer, CancellationToken ct) => throw new NotSupportedException();
        public Task<PostResponseDto> GetPost(int id, string? viewer, CancellationToken ct) => throw new NotSupportedException();
        public Task<List<AuthorResponseDto>> GetAuthors(CancellationToken ct) => throw new NotSupportedException();
    }
}
