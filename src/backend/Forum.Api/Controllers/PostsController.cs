using Forum.Application.Dtos;
using Forum.Application.Dtos.Posts;
using Forum.Domain.Entities;
using Forum.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Forum.Api.Controllers;

[Route("api/v1/posts")]
public sealed class PostsController(IPostService posts) : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<Page<PostResponseDto>>> GetPosts([FromQuery] PostQueryParametersDto query, CancellationToken ct) =>
        Ok(await posts.GetPosts(query, ViewerId, ct));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PostResponseDto>> GetPost(int id, CancellationToken ct) =>
        Ok(await posts.GetPost(id, ViewerId, ct));

    [HttpGet("/api/v1/authors")]
    public async Task<ActionResult<List<AuthorResponseDto>>> GetAuthors(CancellationToken ct) =>
        Ok(await posts.GetAuthors(ct));

    [HttpPost]
    [Authorize]
    [ProducesResponseType<CreatedResourceDto>(StatusCodes.Status201Created)]
    public async Task<ActionResult<CreatedResourceDto>> CreatePost(CreatePostDto request, CancellationToken ct)
    {
        var id = await posts.CreatePost(CurrentUserId, request, ct);
        return CreatedAtAction(nameof(GetPost), new { id }, new CreatedResourceDto(id));
    }

    [HttpPost("{id:int}/likes")]
    [Authorize]
    public async Task<IActionResult> LikePost(int id, CancellationToken ct)
    {
        await posts.LikePost(id, CurrentUserId, ct);
        return NoContent();
    }

    [HttpPost("{id:int}/tags")]
    [Authorize(Roles = RoleNames.Moderator)]
    public async Task<IActionResult> TagPost(int id,
        [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] TagPostDto? request, CancellationToken ct)
    {
        await posts.TagPost(id, CurrentUserId, request, ct);
        return NoContent();
    }
}
