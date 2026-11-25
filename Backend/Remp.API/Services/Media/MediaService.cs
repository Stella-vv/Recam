using Remp.DataAccess.Repositories;
using Remp.Models.Entities;
using Remp.Models.Enums;
using Remp.Models.Logs;

namespace Remp.API.Services.Media;

public class MediaService : IMediaService
{
    private readonly IMediaRepository _mediaRepository;
    private readonly ICaseHistoryRepository	_caseHistoryRepository;
    private readonly ILogger<MediaService> _logger;
    private readonly IMediaStorage _mediaStorage;
    private readonly IListingCaseRepository _listingCaseRepository;

    public MediaService(IMediaRepository mediaRepository, ICaseHistoryRepository caseHistoryRepository, ILogger<MediaService> logger, IMediaStorage mediaStorage, IListingCaseRepository listingCaseRepository)
    {
        _mediaRepository = mediaRepository;
        _caseHistoryRepository = caseHistoryRepository;
        _logger = logger;
        _mediaStorage = mediaStorage;
        _listingCaseRepository = listingCaseRepository;
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

    public async Task<int?> UploadAsync(int listingCaseId, string userId, MediaType mediaType, Stream content, string contentType, CancellationToken cancellationToken)
    {
        var listingCase = await _listingCaseRepository.GetOwnedListingCaseAsync(listingCaseId, userId);
        if(listingCase == null)
        {
            return null;
        }

        var blobName = $"listings/{listingCaseId}/{Guid.NewGuid():N}";
        await _mediaStorage.UploadAsync(blobName, content, contentType, cancellationToken);
        var mediaAsset = new MediaAsset
        {
            ListingCaseId = listingCaseId,
            UserId = userId,
            MediaType = mediaType,
            MediaUrl = blobName,
            UploadedAt = DateTime.UtcNow
        };

        if (cancellationToken.IsCancellationRequested)
        {
            await _mediaStorage.DeleteIfExistsAsync(blobName, CancellationToken.None);
            cancellationToken.ThrowIfCancellationRequested();
        }

        try
        {
            await _mediaRepository.AddAsync(mediaAsset, CancellationToken.None);
        }
        catch(Exception ex)
        {
            try
            {
                var recordExists = await _mediaRepository.ExistsByMediaUrlAsync(blobName, CancellationToken.None);
                if(recordExists)
                {
                    _logger.LogWarning(ex, "Found a media record for {BlobName} after the save operation threw an exception. Keeping the blob.", blobName);
                }
                else
                {
                    try
                    {
                        await _mediaStorage.DeleteIfExistsAsync(blobName, CancellationToken.None);
                    }
                    catch (Exception cleanupEx)
                    {
                        _logger.LogWarning(cleanupEx, "Failed to clean up blob {BlobName} after the media record could not be saved.", blobName);
                    }
                }
            }
            catch (Exception lookupEx)
            {
                _logger.LogWarning(lookupEx, "Could not verify whether the media record for {BlobName} was saved. Keeping the blob.", blobName);
            }

            throw;
        }

        return mediaAsset.Id;
    }
}