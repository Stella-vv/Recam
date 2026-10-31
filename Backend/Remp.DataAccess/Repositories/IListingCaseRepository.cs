using Remp.Models.Entities;

namespace Remp.DataAccess.Repositories;

public interface IListingCaseRepository
{
    public Task AddAsync(ListingCase listingCase);
    public Task<(List<ListingCase> ListingCases, int TotalCount)> GetListingCasesAsync(string userId, bool isPhotographyCompany, int pageNumber, int pageSize);
    public Task<ListingCase?> GetOwnedListingCaseAsync(int id, string userId);
    public Task SaveChangesAsync();
    public Task<ListingCase?> GetAccessibleListingCaseDetailsAsync(int id, string userId, bool isPhotographyCompany);
    public Task<ListingCase?> GetAccessibleListingCaseWithMediaAsync(int id, string userId, bool isPhotographyCompany);
    public Task<bool> CanAccessListingCaseAsync(int id, string userId, bool isPhotographyCompany);
}
