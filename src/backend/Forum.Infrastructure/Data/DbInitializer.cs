using Forum.Domain.Entities;
using Forum.Infrastructure.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Forum.Infrastructure.Data;

public static class DbInitializer
{
    public static async Task Run(AppDbContext db, UserManager<User> users, RoleManager<IdentityRole> roles)
    {
        await db.Database.MigrateAsync();
        foreach (var role in new[] { RoleNames.For(UserRole.Regular), RoleNames.For(UserRole.Moderator) })
        {
            if (!await roles.RoleExistsAsync(role))
                await roles.CreateAsync(new IdentityRole(role));
        }

        var names = new[]
        {
            ("alex", "Alex Chen"), ("priya", "Priya Desai"), ("marcus", "Marcus Lee"),
            ("taylor", "Taylor Kim"), ("moderator", "Morgan")
        };
        foreach (var (key, name) in names)
        {
            if (await users.FindByEmailAsync($"{key}@example.test") is not null) continue;
            var user = new User { Id = key, UserName = $"{key}@example.test", Email = $"{key}@example.test", DisplayName = name };
            var result = await users.CreateAsync(user, "ForumDemo!2026");
            if (!result.Succeeded) throw new InvalidOperationException(string.Join("; ", result.Errors.Select(e => e.Description)));
            await users.AddToRoleAsync(user, RoleNames.For(key == "moderator" ? UserRole.Moderator : UserRole.Regular));
        }
        
        if (await db.Posts.AnyAsync()) return;
        var titles = new[]
        {
            "Getting started with the verification API", "Handling webhook retries safely",
            "Testing in the sandbox environment", "Understanding liveness results"
        };
        var bodies = new[]
        {
            "I’m looking for the recommended way to set up the verification API in our staging environment. Are there any configuration steps I should be aware of?",
            "We’re seeing duplicate webhook deliveries in certain scenarios. What’s the recommended approach for making our integration idempotent?",
            "I believe sandbox responses can be used as production verification evidence. Can someone confirm this assumption?",
            "Can someone explain the different liveness result states and how we should handle borderline scores in production?"};

        for (var i = 0; i < titles.Length; i++)
        {
            var post = new Post
            {
                AuthorId = names[i].Item1,
                Title = titles[i],
                Body = bodies[i],
                CreatedAt = DateTime.UtcNow.AddDays(-i - 1)
            };
            post.Comments.Add(new Comment
            {
                AuthorId = "moderator",
                Body = i == 2
                    ? "This is demonstration content and is intentionally flagged. Sandbox data must not be treated as real verification evidence."
                    : "Thanks for raising this. Please include a sanitised example so the team can help."
            });
            post.Likes.Add(new Like { UserId = "moderator" });
            if (i == 2) post.Tags.Add(new ModerationTag { ModeratorId = "moderator" });
            db.Posts.Add(post);
        }
        await db.SaveChangesAsync();
    }
}
