using Solver.Core.Base;

namespace Solver.Core;

public class BreadthFirstSolver<TState, TStep>(Func<TState, IEnumerable<TStep>> generateSteps, Func<TState, TStep, TState> performStep,
        Func<TState, bool> solvedTest, IEqualityComparer<TState>? comparer = null)
    : Solver<TState, TStep>(generateSteps, performStep, solvedTest, comparer)
{
    private readonly Queue<Solution<TState, TStep>> _solutions = new();

    protected override Solution<TState, TStep> GetNextSolution() => _solutions.Dequeue();
    protected override bool CanGetNextSolution() => _solutions.Count > 0;
    protected override void StoreSolution(Solution<TState, TStep> solution) => _solutions.Enqueue(solution);
}