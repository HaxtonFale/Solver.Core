using Solver.Core.Base;

namespace Solver.Core;

public class AStarSolver<TState, TStep, TPriority>(Func<TState, IEnumerable<TStep>> generateSteps, Func<TState, TStep, TState> performStep,
        Func<TState, bool> solvedTest, Func<TState, TPriority> heuristic, IEqualityComparer<TState>? comparer = null)
    : Solver<TState, TStep>(generateSteps, performStep, solvedTest, comparer)
{
    private readonly PriorityQueue<Solution<TState, TStep>, TPriority> _queue = new();

    protected override Solution<TState, TStep> GetNextSolution() => _queue.Dequeue();
    protected override bool CanGetNextSolution() => _queue.Count > 0;
    protected override void StoreSolution(Solution<TState, TStep> solution)
    {
        var priority = heuristic(solution.State);
        _queue.Enqueue(solution, priority);
    }
}