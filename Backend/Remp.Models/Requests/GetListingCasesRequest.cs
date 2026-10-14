using System.ComponentModel.DataAnnotations;

namespace Remp.Models.Requests;

public class GetListingCasesRequest
{
    [Range(1,int.MaxValue)]
    public int PageNumber {get; set; } = 1;
    [Range(1,100)]
    public int PageSize {get; set; } = 10;

}