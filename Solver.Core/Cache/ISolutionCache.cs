namespace Solver.Core.Cache;

public interface ISolutionCache<TState, TStep>
{
    Solution<TState, TStep> GetSolution(Guid solutionId);

    void StoreSolution(Solution<TState, TStep> solution);
}