using Forum.Application.Dtos;
using Forum.Application.Dtos.Comments;
using Forum.Infrastructure.Data;
using Forum.Domain.Entities;
using Forum.Application.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Forum.Infrastructure.Repositories;

public sealed class CommentRepository(AppDbContext db) : ICommentRepository
{
    public async Task<Page<CommentResponseDto>> GetComments(int id, int page, int pageSize, CancellationToken ct)
    {
        var query = db.Comments.AsNoTracking().Where(c => c.PostId == id);
        var total = await query.CountAsync(ct);
        var items = await query.OrderBy(c => c.CreatedAt).ThenBy(c => c.Id).Skip((page - 1) * pageSize).Take(pageSize)
            .Select(c => new CommentResponseDto(c.Id, db.Users.Where(u => u.Id == c.AuthorId).Select(u => u.DisplayName).First(), c.Body, c.CreatedAt)).ToListAsync(ct);
        return new(items, total, page, pageSize);
    }

    public async Task<int> Add(Comment comment, CancellationToken ct)
    {
        db.Comments.Add(comment);
        await db.SaveChangesAsync(ct);
        return comment.Id;
    }
}
