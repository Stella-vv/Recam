using Remp.DataAccess.Data;
using Remp.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Remp.DataAccess.Repositories;

public class CaseContactRepository : ICaseContactRepository
{
    private readonly ApplicationDbContext _context;

    public CaseContactRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<CaseContact>> 	GetByListingCaseIdAsync(int listingCaseId)
    {
        List<CaseContact> contacts = await _context.CaseContacts.AsNoTracking().Where(contact => contact.ListingCaseId == listingCaseId).OrderBy(contact => contact.ContactId).ToListAsync();
        return contacts;
    }
}