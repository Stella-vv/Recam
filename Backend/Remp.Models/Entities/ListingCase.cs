using Remp.Models.Enums;

namespace Remp.Models.Entities;

public class ListingCase
{
    public int Id { get; set; }

    public int Bedrooms { get; set; }
    public int Bathrooms { get; set; }

    public int Garages { get; set; }

    public double FloorArea { get; set; }

    public bool IsDeleted { get; set; }

    public decimal Longitude { get; set; }

    public decimal Latitude { get; set; }

    public DateTime CreatedAt { get; set; }

    public string? Title { get; set; }

    public string? Description { get; set; }

    public required string Street { get; set; }

    public required string City { get; set; }

    public required string State { get; set; }

    public int Postcode { get; set; }

    public double Price { get; set; }

    public PropertyType PropertyType { get; set; }

    public SaleCategory SaleCategory { get; set; }

    public ListingCaseStatus ListingCaseStatus { get; set; }

    public required string UserId { get; set; }

    public ICollection<CaseContact> CaseContacts { get; set; } = new List<CaseContact>();
}