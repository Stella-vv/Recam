
namespace Remp.API.Services.Media;

public interface IMediaService
{
    public Task<bool> DeleteAsync(int id, string userId);
}