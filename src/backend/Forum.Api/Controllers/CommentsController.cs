using Forum.Application.Dtos;
using Forum.Application.Dtos.Comments;
using Forum.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Forum.Api.Controllers;

[Route("api/v1/posts/{id:int}/comments")]
public sealed class CommentsController(ICommentService comments) : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<Page<CommentResponseDto>>> GetComments(int id, CancellationToken ct,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 10) =>
        Ok(await comments.GetComments(id, page, pageSize, ct));

    [HttpPost]
    [Authorize]
    [ProducesResponseType<CreatedResourceDto>(StatusCodes.Status201Created)]
    public async Task<ActionResult<CreatedResourceDto>> CreateComment(int id, CreateCommentDto request, CancellationToken ct)
    {
        var commentId = await comments.CreateComment(id, CurrentUserId, request, ct);
        // Location points to this post's comments collection; the body identifies the new comment.
        return CreatedAtAction(nameof(GetComments), new { id }, new CreatedResourceDto(commentId));
    }
}
