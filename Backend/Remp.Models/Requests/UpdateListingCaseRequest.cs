using Remp.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

namespace Remp.Models.Requests;

public class UpdateListingCaseRequest
{
    [Required]
    public required string Street { get; set; }
    [Required]
    public required string City { get; set; }
    [Required]
    public required string State { get; set; }
    [Range(0,9999)]
    public int Postcode { get; set; }
    [Range(0,int.MaxValue)]
    public int Bedrooms { get; set; }
    [Range(0,int.MaxValue)]
    public int Bathrooms { get; set; }
    [Range(0,int.MaxValue)]
    public int Garages { get; set; }
    [Range(0,double.MaxValue)]
    public double FloorArea { get; set; }
    [EnumDataType(typeof(PropertyType))]
    public required PropertyType PropertyType { get; set; }
    [EnumDataType(typeof(SaleCategory))]
    public required SaleCategory SaleCategory { get; set; }


}