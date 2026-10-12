namespace Remp.Models.Requests;
using System.ComponentModel.DataAnnotations;

public class GetUsersRequest
{
    [Range(1,int.MaxValue)]
    public int PageNumber { get; set; } = 1;
    [Range(1,100)]
    public int PageSize { get; set; } = 10;
}