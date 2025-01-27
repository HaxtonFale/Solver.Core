using Microsoft.Extensions.Logging;

namespace Solver.Core.Cache;

internal class InMemoryCache<TState, TStep>(ILogger<InMemoryCache<TState, TStep>> logger) : ISolutionCache<TState, TStep>
{
    private readonly IDictionary<Guid, Solution<TState, TStep>> _cache = new Dictionary<Guid, Solution<TState, TStep>>();

    #region ISolutionCache<TState,TStep>

    public Solution<TState, TStep> GetSolution(Guid solutionId)
    {
        logger.LogTrace("Retrieving solution under ID {Id:D}", solutionId);
        if (_cache.TryGetValue(solutionId, out var solution))
        {
            logger.LogTrace("Solution located. Returning...");
            return solution;
        }

        logger.LogTrace("Could not locate solution.");
        throw new ArgumentException($"Solution with ID {solutionId:D} has not been cached yet.", nameof(solutionId));
    }

    public void RememberSolution(Solution<TState, TStep> solution)
    {
        var key = solution.Id;
        logger.LogTrace("Caching solution with ID {Id:D}", key);
        if (_cache.ContainsKey(key))
        {
            throw new InvalidOperationException("Solution ID collision detected");
        }
        _cache[key] = solution;
    }

    #endregion
}