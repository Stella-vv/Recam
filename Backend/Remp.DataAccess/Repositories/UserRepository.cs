using Remp.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using Remp.Models.Responses;


namespace Remp.DataAccess.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;

    public UserRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> CountActiveUsersAsync()
    {
        return await _context.Users.Where(user => user.IsDeleted == false).CountAsync();
    }

    public async Task<List<UserResponse>> GetActiveUsersPageAsync(int skipCount, int pageSize)
    {
        var query = _context.Users.Where(user => !user.IsDeleted);
        var orderedQuery = query.OrderBy(user => user.Id);

        var pageQuery = orderedQuery.Skip(skipCount).Take(pageSize);
        var responseQuery = pageQuery.Select( user => new UserResponse
        {
            UserId = user.Id,
            Email = user.Email ?? string.Empty,
            AgentFirstName = user.Agent != null ? user.Agent.AgentFirstName : string.Empty,
            AgentLastName = user.Agent != null ? user.Agent.AgentLastName : string.Empty,
            CompanyName = user.Agent != null ? user.Agent.CompanyName : null
        } );
        return await responseQuery.ToListAsync();
    }

    public async Task<bool> ActiveUserExistsAsync(string userId)
    {
        return await _context.Users.AnyAsync(user => user.Id == userId && user.IsDeleted == false);
    }

    public async Task<List<int>> GetAssignedListingIdsAsync(string userId)
    {
        return await _context.ListingCases.AsNoTracking().Where(listingCase => listingCase.IsDeleted == false
                                                                && listingCase.AgentListingCases.Any(assignment => assignment.AgentId == userId))
                                                                .OrderBy(listingCase => listingCase.Id).Select(listingCase => listingCase.Id).ToListAsync();
    }

}