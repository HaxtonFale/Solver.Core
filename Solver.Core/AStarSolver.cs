using Solver.Core.Base;
using Solver.Core.Cache;

namespace Solver.Core;

public class AStarSolver<TState, TStep, TPriority>(Func<TState, IEnumerable<TStep>> generateSteps, Func<TState, TStep, TState> performStep,
        Func<TState, bool> solvedTest, Func<TState, TPriority> heuristic, ISolutionCache<TState, TStep> solutionCache, IEqualityComparer<TState>? comparer = null)
    : Solver<TState, TStep>(generateSteps, performStep, solvedTest, comparer)
{
    private readonly PriorityQueue<Guid, TPriority> _queue = new();

    protected override Solution<TState, TStep> GetNextSolution() => solutionCache.GetSolution(_queue.Dequeue());

    protected override bool CanGetNextSolution() => _queue.Count > 0;
    protected override void EnqueueSolution(Solution<TState, TStep> solution)
    {
        var priority = heuristic(solution.State);
        _queue.Enqueue(solution.Id, priority);
        solutionCache.StoreSolution(solution);
    }

    protected internal override IEnumerable<Solution<TState, TStep>> GetAllSolutions()
    {
        foreach (var (element, _) in _queue.UnorderedItems) yield return solutionCache.GetSolution(element);
    }
}