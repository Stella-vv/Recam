using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Remp.DataAccess.Data;
using Remp.DataAccess.Repositories;
using Remp.Models.Responses;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Remp.Models.Entities;
using Remp.Models.Logs;

namespace Remp.API.Controllers;


[ApiController]
[Route("api/media")]
[Authorize(Roles = "photographyCompany")]
public class MediaController : ControllerBase
{
    private readonly ApplicationDbContext	_context;
    private readonly ICaseHistoryRepository	_caseHistoryRepository;
    private readonly ILogger<MediaController>	_logger;

    public MediaController(ApplicationDbContext	context, ICaseHistoryRepository	caseHistoryRepository, ILogger<MediaController>	logger){
        _context = context;
        _caseHistoryRepository = caseHistoryRepository;
        _logger = logger;
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(ApiResponse<object>.Fail("Invalid user identity."));
        }

        MediaAsset? mediaAsset = await _context.MediaAssets.FirstOrDefaultAsync( media => media.Id == id && media.IsDeleted == false &&
                                                                           media.ListingCase != null && media.ListingCase.IsDeleted == false && media.ListingCase.UserId == userId);
        if(mediaAsset == null)
        {
            return NotFound(ApiResponse<object>.Fail("Media file not found."));
        }

        _context.MediaAssets.Remove(mediaAsset);
        await _context.SaveChangesAsync();

        CaseHistoryLog log = new CaseHistoryLog()
        {
            ListingCaseId	= mediaAsset.ListingCaseId,
            UserId = userId,
            Action = "MediaDeleted",
            CreatedAt	= DateTime.UtcNow
        };

        try
        {
            await _caseHistoryRepository.AddAsync(log);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save media deletion history for media {MediaId}.", mediaAsset.Id);
        }

        return NoContent();
    }

}