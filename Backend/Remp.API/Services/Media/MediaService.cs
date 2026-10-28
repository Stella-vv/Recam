using Remp.DataAccess.Repositories;
using Remp.Models.Entities;
using Remp.Models.Logs;

namespace Remp.API.Services.Media;

public class MediaService : IMediaService
{
    private readonly IMediaRepository _mediaRepository;
    private readonly ICaseHistoryRepository	_caseHistoryRepository;
    private readonly ILogger<MediaService> _logger;

    public MediaService(IMediaRepository mediaRepository, ICaseHistoryRepository	caseHistoryRepository, ILogger<MediaService> logger)
    {
        _mediaRepository = mediaRepository;
        _caseHistoryRepository = caseHistoryRepository;
        _logger = logger;
    }

    public async Task<bool> DeleteAsync(int id, string userId)
    {
        MediaAsset? mediaAsset = await _mediaRepository.GetOwnedMediaAsync(id, userId);
        if(mediaAsset == null)
        {
            return false;
        }
        await _mediaRepository.DeleteAsync(mediaAsset);

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

        return true;
    }
}