using Remp.Models.Enums;

namespace Remp.Models.Entities;

public class MediaAsset
{
    public int Id { get; set; }
    public MediaType MediaType { get; set; }
    public required string MediaUrl { get; set; }

    public DateTime UploadedAt { get; set; }

    public bool IsSelect { get; set; }

    public bool IsHero { get; set; }

    public bool IsDeleted { get; set; }

    public int ListingCaseId { get; set; }

    public required string UserId { get; set; }

    public ApplicationUser? User { get; set; }

    public ListingCase? ListingCase { get; set; } 
}