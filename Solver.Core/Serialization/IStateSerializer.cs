
namespace Solver.Core.Serialization;

public interface IStateSerializer<TState, TStep>
{
    public int SerializedStepLength { get; }
    public byte[] SerializeStep(TStep step);
    public byte[] SerializeState(TState state);
    public TStep DeserializeStep(ReadOnlySpan<byte> buffer);
    public TState DeserializeState(ReadOnlySpan<byte> buffer);
}