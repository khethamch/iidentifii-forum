using Forum.Domain.Entities;
using Forum.Infrastructure.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Forum.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<User>(options)
{
    public DbSet<Post> Posts => Set<Post>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<Like> Likes => Set<Like>();
    public DbSet<ModerationTag> Tags => Set<ModerationTag>();
    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);
        b.Entity<User>().Property(u => u.DisplayName).HasMaxLength(100);
        b.Entity<Post>().Property(p => p.Title).HasMaxLength(180);
        b.Entity<Post>().Property(p => p.Body).HasMaxLength(10000);
        b.Entity<Comment>().Property(c => c.Body).HasMaxLength(3000);
        b.Entity<ModerationTag>().Property(t => t.Label).HasMaxLength(100);
        b.Entity<Post>().HasIndex(p => new { p.CreatedAt, p.Id });
        b.Entity<Post>().HasIndex(p => new { p.AuthorId, p.CreatedAt });
        b.Entity<Comment>().HasIndex(c => new { c.PostId, c.CreatedAt, c.Id });
        b.Entity<Like>().HasKey(l => new { l.PostId, l.UserId });
        b.Entity<ModerationTag>().HasIndex(t => new { t.PostId, t.Label }).IsUnique();
        b.Entity<ModerationTag>().HasIndex(t => t.Label);
        b.Entity<Post>().HasOne<User>().WithMany().HasForeignKey(p => p.AuthorId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<Comment>().HasOne<User>().WithMany().HasForeignKey(p => p.AuthorId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<Like>().HasOne<User>().WithMany().HasForeignKey(p => p.UserId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<ModerationTag>().HasOne<User>().WithMany().HasForeignKey(p => p.ModeratorId).OnDelete(DeleteBehavior.Restrict);
    }
}
