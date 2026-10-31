using Remp.DataAccess.Data;
using Remp.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Remp.DataAccess.Repositories;

public class ListingCaseRepository : IListingCaseRepository
{
    private readonly ApplicationDbContext _context;

    public ListingCaseRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(ListingCase listingCase)
    {
        _context.ListingCases.Add(listingCase);
        await _context.SaveChangesAsync();
    }

    public async Task<(List<ListingCase> ListingCases, int TotalCount)> GetListingCasesAsync(string userId, bool isPhotographyCompany, int pageNumber, int pageSize)
    {
        var query = _context.ListingCases.AsNoTracking().Where(listingCase => listingCase.IsDeleted == false);
        if(isPhotographyCompany == true)
        {
          query = query.Where(listingCase => listingCase.UserId == userId);
        }
        else
        {
          query = query.Where(listingCase => listingCase.AgentListingCases.Any(assignment => assignment.AgentId == userId));
        }

        int totalCount = await query.CountAsync();

        var orderedQuery = query.OrderByDescending(listingCase => listingCase.CreatedAt).ThenByDescending(listingCase => listingCase.Id);
        long skipCount = (long)(pageNumber - 1) * pageSize;

        List<ListingCase> listingCases = new List<ListingCase>();
        if(skipCount < totalCount)
        {
            listingCases = await orderedQuery.Skip((int)skipCount).Take(pageSize).ToListAsync();
        }

        return (listingCases, totalCount);
    }

    public async Task<ListingCase?> GetOwnedListingCaseAsync(int id, string userId)
    {
        return await _context.ListingCases.FirstOrDefaultAsync(listingcase => listingcase.Id ==id && listingcase.UserId == userId && listingcase.IsDeleted == false);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

    public async Task<ListingCase?> GetAccessibleListingCaseDetailsAsync(int id, string userId, bool isPhotographyCompany)
    {
        var query = _context.ListingCases.AsNoTracking().Where(listingcase => listingcase.Id == id && listingcase.IsDeleted ==false);
        if(isPhotographyCompany)
        {
            query = query.Where( listingcase => listingcase.UserId == userId);
        }
        else
        {
            query = query.Where( listingcase => listingcase.AgentListingCases.Any(assignment =>assignment.AgentId == userId));
        }

        ListingCase? listingCase = await query.Include(listingcase => listingcase.MediaAssets.Where(media => media.IsDeleted == false))
                           .Include(listingcase => listingcase.AgentListingCases)
                           .ThenInclude(assignment => assignment.Agent)
                           .ThenInclude(agent => agent!.User)
                           .FirstOrDefaultAsync();
        return listingCase;
    }

    public async Task<ListingCase?> GetAccessibleListingCaseWithMediaAsync(int id, string userId, bool isPhotographyCompany)
    {
        var query = _context.ListingCases.AsNoTracking().Where(listingcase => listingcase.Id == id && listingcase.IsDeleted == false);
        if (isPhotographyCompany)
        {
            query = query.Where( listingcase => listingcase.UserId == userId);
        }
        else
        {
            query = query.Where( listingcase => listingcase.AgentListingCases.Any(assignment =>assignment.AgentId == userId));
        }
        return await query.Include(listingcase => listingcase.MediaAssets.Where(media => media.IsDeleted == false)).FirstOrDefaultAsync();

    }

    public async Task<bool> CanAccessListingCaseAsync(int id, string userId, bool isPhotographyCompany)
    {
        var query = _context.ListingCases.AsNoTracking().Where(listingcase => listingcase.Id == id && listingcase.IsDeleted == false);
        if(isPhotographyCompany)
        {
            query = query.Where( listingcase => listingcase.UserId == userId);
        }
        else
        {
            query = query.Where( listingcase => listingcase.AgentListingCases.Any(assignment =>assignment.AgentId == userId));
        }

        return await query.AnyAsync();
    }


}