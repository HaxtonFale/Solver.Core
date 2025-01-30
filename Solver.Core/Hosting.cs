using Microsoft.Extensions.DependencyInjection;
using Solver.Core.Cache;
using Solver.Core.IO;
using Solver.Core.Serialization;

namespace Solver.Core;

public static class Hosting
{
    public static IServiceCollection AddDiskCache<TState, TStep>(this IServiceCollection services,
        string configurationSectionName = DiskCacheOptions.ConfigurationSectionName) =>
        services
            .AddSingleton<ISolutionCache<TState, TStep>, DiskCache<TState, TStep>>()
            .AddSingleton<IFilenameGenerator, FilenameGenerator>()
            .AddSingleton<SolutionSerializer<TState, TStep>>()
            .AddOptions<DiskCacheOptions>().BindConfiguration(configurationSectionName).Services;
}