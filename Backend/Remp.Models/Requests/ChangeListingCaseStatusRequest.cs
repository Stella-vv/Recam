namespace Remp.Models.Requests;
using Remp.Models.Enums;
using System.ComponentModel.DataAnnotations;

public class ChangeListingCaseStatusRequest
{
    [Required]
    [EnumDataType(typeof(ListingCaseStatus))]
    public ListingCaseStatus? ListingCaseStatus {get; set;}
}