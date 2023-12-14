
namespace Solver.Core.Serialization;

public interface IStateSerializer<TState, TStep>
{
    public int SerializedStepLength { get; }
    public int SerializeStep(TStep step, byte[] bytes);
    public int SerializeState(TState state, byte[] bytes);
    public TStep DeserializeStep(ReadOnlySpan<byte> buffer);
    public TState DeserializeState(ReadOnlySpan<byte> buffer);
}