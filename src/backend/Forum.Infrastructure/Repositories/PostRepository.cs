using Forum.Application.Dtos;
using Forum.Application.Dtos.Posts;
using Forum.Infrastructure.Data;
using Forum.Domain.Entities;
using Forum.Domain.Exceptions;
using Forum.Application.Enums;
using Forum.Application.Interfaces.Repositories;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Forum.Infrastructure.Repositories;

public sealed class PostRepository(AppDbContext db) : IPostRepository
{
    private IQueryable<PostResponseDto> Project(IQueryable<Post> posts, string? viewer) =>
        posts.Select(p => new PostResponseDto(p.Id, p.AuthorId,
            db.Users.Where(u => u.Id == p.AuthorId).Select(u => u.DisplayName).First(),
            p.Title, p.Body, p.CreatedAt, p.Likes.Count, p.Comments.Count,
            p.Likes.Any(l => l.UserId == viewer),
            p.Tags.Select(t => new ModerationTagResponseDto(t.Label,
                db.Users.Where(u => u.Id == t.ModeratorId).Select(u => u.DisplayName).First(), t.CreatedAt)).ToList()));

    public async Task<Page<PostResponseDto>> GetPosts(PostQueryParametersDto q, SortByOptions sort, string? viewer, CancellationToken ct)
    {
        var posts = db.Posts.AsNoTracking().AsQueryable();
        if (!string.IsNullOrEmpty(q.Author)) posts = posts.Where(p => p.AuthorId == q.Author);
        if (q.From != null) posts = posts.Where(p => p.CreatedAt >= q.From);
        if (q.To != null) posts = posts.Where(p => p.CreatedAt < q.To);
        if (q.Tag == "flagged") posts = posts.Where(p => p.Tags.Any());
        if (q.Tag == "unflagged") posts = posts.Where(p => !p.Tags.Any());
        var total = await posts.CountAsync(ct);
        posts = sort switch
        {
            SortByOptions.DateDescending => posts.OrderByDescending(p => p.CreatedAt).ThenByDescending(p => p.Id),
            SortByOptions.DateAscending => posts.OrderBy(p => p.CreatedAt).ThenBy(p => p.Id),
            SortByOptions.Likes => posts.OrderByDescending(p => p.Likes.Count).ThenByDescending(p => p.Id),
            _ => throw new ForumException("Sort must be newest, oldest or likes.")
        };
        return new(await Project(posts.Skip((q.Page - 1) * q.PageSize).Take(q.PageSize), viewer).ToListAsync(ct), total, q.Page, q.PageSize);
    }

    public async Task<PostResponseDto> GetPost(int id, string? viewer, CancellationToken ct) =>
        await Project(db.Posts.AsNoTracking().Where(p => p.Id == id), viewer).SingleOrDefaultAsync(ct)
        ?? throw new ForumException("Post not found.", ForumError.NotFound);

    public Task<List<AuthorResponseDto>> GetAuthors(CancellationToken ct) => db.Users.AsNoTracking()
        .Where(u => db.Posts.Any(p => p.AuthorId == u.Id)).OrderBy(u => u.DisplayName)
        .Select(u => new AuthorResponseDto(u.Id, u.DisplayName)).ToListAsync(ct);

    public async Task<int> Add(Post post, CancellationToken ct)
    {
        db.Posts.Add(post);
        await db.SaveChangesAsync(ct);
        return post.Id;
    }

    public async Task<Post> RequirePost(int id, CancellationToken ct) =>
        await db.Posts.SingleOrDefaultAsync(p => p.Id == id, ct) ?? throw new ForumException("Post not found.", ForumError.NotFound);

    public async Task AddLike(Like like, CancellationToken ct)
    {
        db.Likes.Add(like);
        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 2601 or 2627 })
        {
            throw new ForumException("You already liked this post.", ForumError.Conflict);
        }
    }

    public async Task AddTag(ModerationTag tag, CancellationToken ct)
    {
        db.Tags.Add(tag);
        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 2601 or 2627 })
        {
            throw new ForumException("This post is already flagged.", ForumError.Conflict);
        }
    }
}
