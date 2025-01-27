namespace Solver.Core.Cache;

public interface ISolutionCache<TState, TStep>
{
    Solution<TState, TStep> GetSolution(Guid solutionId);

    void RememberSolution(Solution<TState, TStep> solution);
}