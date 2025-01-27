using System.IO.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Solver.Core.Cache;

public class DiskCache<TState, TStep>(IFileSystem fileSystem, IOptions<DiskCacheOptions> options, ILogger<DiskCache<TState, TStep>> logger) : ISolutionCache<TState, TStep>
{


    #region ISolutionCache<TState,TStep>

    public Solution<TState, TStep> GetSolution(Guid solutionId)
    {
        throw new NotImplementedException();
    }

    public void RememberSolution(Solution<TState, TStep> solution)
    {
        throw new NotImplementedException();
    }

    #endregion
}