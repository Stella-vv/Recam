
using Remp.Models.Enums;

namespace Remp.API.Services.Media;

public interface IMediaService
{
    public Task<bool> DeleteAsync(int id, string userId);
    public Task<int?> UploadAsync(int listingCaseId, string userId, MediaType mediaType, Stream content, string contentType, CancellationToken cancellationToken);
}