using System.Dynamic;
using Remp.Models.Enums;

namespace Remp.Models.Entities;

public class MediaAsset
{
    public int Id { get; set; }
    public MediaType MediaType { get; set; }
    public required string MediaUrl { get; set; }
}