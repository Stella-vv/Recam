using Microsoft.AspNetCore.Identity;

namespace Remp.Models.Entities;

public class ApplicationUser: IdentityUser
{
    public bool IsDeleted { get; set; }

    public DateTime CreatedAt { get; set; }

    public ICollection<ListingCase> CreatedListingCases { get; set; } = new List<ListingCase>() ;

    public ICollection<MediaAsset> UploadedMediaAssets { get; set; } = new List<MediaAsset>();

    public Agent? Agent { get; set; }

    public PhotographyCompany? PhotographyCompany { get; set; }
}