using Remp.Models.Entities;

namespace Remp.DataAccess.Repositories;

public interface IMediaRepository
{
    public Task<MediaAsset?> GetOwnedMediaAsync(int id, string userId);
    public Task DeleteAsync(MediaAsset mediaAsset);
}