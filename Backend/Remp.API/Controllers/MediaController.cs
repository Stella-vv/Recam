using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Remp.Models.Responses;
using System.Security.Claims;
using Remp.API.Services.Media;

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

}