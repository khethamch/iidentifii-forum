using Forum.Application.Interfaces.Services;
using Forum.Application.Services;

namespace Forum.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddForumApplication(this IServiceCollection services)
    {
        services.AddScoped<IPostService, PostService>();
        services.AddScoped<ICommentService, CommentService>();
        return services;
    }
}
