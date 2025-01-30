using System.ComponentModel.DataAnnotations;

namespace Solver.Core.Cache;

public class DiskCacheOptions
{
    public const string ConfigurationSectionName = "Cache";

    public string? Root { get; init; }
}