using Solver.Core.Base;

namespace Solver.Core;

public class BreadthFirstSolver<TState, TStep>(Func<TState, IEnumerable<TStep>> generateSteps, Func<TState, TStep, TState> performStep,
        Func<TState, bool> solvedTest, IEqualityComparer<TState>? comparer = null)
    : Solver<TState, TStep>(generateSteps, performStep, solvedTest, comparer)
{
    private readonly Queue<Solution<TState, TStep>> _queue = new();

    protected override Solution<TState, TStep> GetNextSolution() => _queue.Dequeue();
    protected override bool CanGetNextSolution() => _queue.Count > 0;
    protected override void EnqueueSolution(Solution<TState, TStep> solution) => _queue.Enqueue(solution);
    protected internal override IEnumerable<Solution<TState, TStep>> GetAllSolutions() => _queue;
}