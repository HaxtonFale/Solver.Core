namespace Solver.Core.Base;

public abstract class Solver<TState, TStep>(Func<TState, IEnumerable<TStep>> generateSteps, Func<TState, TStep, TState> performStep,
    Func<TState, bool> solvedTest, IEqualityComparer<TState>? comparer = null)
{
    protected abstract Solution<TState, TStep> GetNextSolution();
    protected abstract bool CanGetNextSolution();
    protected abstract void StoreSolution(Solution<TState, TStep> solution);
    protected internal abstract IEnumerable<Solution<TState, TStep>> GetAllSolutions();

    public Solution<TState, TStep>? TrySolve(TState initialState, CancellationToken token = default)
    {
        StoreSolution(new Solution<TState, TStep>(initialState));

        var visitedStates = comparer == null
            ? new HashSet<TState> {initialState}
            : new HashSet<TState>(comparer) {initialState};

        while (CanGetNextSolution() && !token.IsCancellationRequested)
        {
            var solution = GetNextSolution();
            foreach (var step in generateSteps(solution.State))
            {
                var state = performStep(solution.State, step);
                if (!visitedStates.Add(state)) continue;
                var newSolution = solution.AddStep(state, step);
                if (solvedTest(state)) return newSolution;
                StoreSolution(newSolution);
            }
        }

        return null;
    }
}