using System.ComponentModel.DataAnnotations;

namespace Solver.Core.Cache;

public class DiskCacheOptions
{
    public const string ConfigurationSectionName = "Cache";

    [Required]
    public string Root { get; init; }
}