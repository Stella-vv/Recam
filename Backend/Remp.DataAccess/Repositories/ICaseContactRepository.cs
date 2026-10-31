using Remp.Models.Entities;

namespace Remp.DataAccess.Repositories;

public interface ICaseContactRepository
{
    public Task<List<CaseContact>> 	GetByListingCaseIdAsync(int listingCaseId);
}