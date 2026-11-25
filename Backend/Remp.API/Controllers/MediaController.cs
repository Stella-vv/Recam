using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Remp.Models.Responses;
using System.Security.Claims;
using Remp.API.Services.Media;
using Remp.Models.Enums;

namespace Remp.API.Controllers;


[ApiController]
[Route("api/media")]
[Authorize(Roles = "photographyCompany")]
public class MediaController : ControllerBase
{
    private readonly IMediaService _mediaService;

    public MediaController(IMediaService mediaService){
        _mediaService = mediaService;
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(ApiResponse<object>.Fail("Invalid user identity."));
        }

        bool deleted = await _mediaService.DeleteAsync(id, userId);
        if(deleted == false)
        {
            return NotFound(ApiResponse<object>.Fail("Media file not found."));
        }

        return NoContent();
    }

    [HttpPost("~/api/listings/{listingCaseId:int}/media")]
    public async Task<IActionResult> Upload(int listingCaseId, IFormFile? file, [FromForm] MediaType? mediaType, CancellationToken cancellationToken)
    {
        if(file == null || file.Length == 0)
        {
            return BadRequest(ApiResponse<object>.Fail("A non-empty file is required."));
        }

        if(!mediaType.HasValue || !Enum.IsDefined(mediaType.Value))
        {
            return BadRequest(ApiResponse<object>.Fail("A valid media type is required."));
        }

        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(ApiResponse<object>.Fail("Invalid user identity."));
        }

        using var content = file.OpenReadStream();

        int? mediaId = await _mediaService.UploadAsync(listingCaseId, userId, mediaType.Value, content, file.ContentType, cancellationToken);
        if(mediaId == null)
        {
            return NotFound(ApiResponse<object>.Fail("Listing case not found."));
        }

        return StatusCode(StatusCodes.Status201Created, ApiResponse<object>.Ok(new { mediaId = mediaId.Value }));

    }

}