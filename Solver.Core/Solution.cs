namespace Solver.Core;

public class Solution<TState, TStep>
{
    public TState State { get; init; }
    public (Solution<TState, TStep> Solution, TStep Step)? PreviousStep { get; init; }
    public uint Length { get; }

    public Solution(TState state)
    {
        State = state;
        Length = 0;
    }

    private Solution(TState state, (Solution<TState, TStep> Solution, TStep step) previousStep)
    {
        State = state;
        PreviousStep = previousStep;
        Length = 1 + previousStep.Solution.Length;
    }

    public Solution<TState, TStep> AddStep(TState state, TStep step) => new(state, (this, step));
}