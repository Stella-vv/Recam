using Remp.DataAccess.Data;
using Remp.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Remp.DataAccess.Repositories;

public class MediaRepository : IMediaRepository
{
    private readonly ApplicationDbContext _context;

    public MediaRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<MediaAsset?> GetOwnedMediaAsync(int id, string userId)
    {
        MediaAsset? mediaAsset = await _context.MediaAssets.FirstOrDefaultAsync(media => media.Id == id && media.IsDeleted == false
                                                          && media.ListingCase != null && media.ListingCase.IsDeleted == false && media.ListingCase.UserId == userId);
        return mediaAsset;
    }

    public async Task DeleteAsync(MediaAsset mediaAsset)
    {
        _context.MediaAssets.Remove(mediaAsset);
        await _context.SaveChangesAsync();
    }

    public async Task<List<MediaAsset>> GetByListingCaseIdAsync(int listingCaseId)
    {
        return await _context.MediaAssets.Where(media => media.ListingCaseId == listingCaseId && media.IsDeleted == false).ToListAsync();
    }
}