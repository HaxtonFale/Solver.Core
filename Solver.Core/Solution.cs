namespace Solver.Core;

public class Solution<TState, TStep>
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public TState State { get; init; }

    public (Guid Id, TStep Step)? Previous { get; init; }

    public uint Length { get; internal init; }

    public Solution(TState state)
    {
        State = state;
        Length = 0;
    }

    public Solution(TState state, Solution<TState, TStep> previousSolution, TStep previousStep)
    {
        State = state;
        Previous = (previousSolution.Id, previousStep);
        Length = 1 + previousSolution.Length;
    }

    public Solution<TState, TStep> AddStep(TState state, TStep step) => new(state, this, step);
}