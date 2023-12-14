using System.Runtime.CompilerServices;

namespace Solver.Core.Serialization;

public class SolutionSerializer<TState, TStep>(IStateSerializer<TState, TStep> stateSerializer)
{
    public async Task Serialize(IEnumerable<Solution<TState, TStep>> solutions, Stream stream, CancellationToken token = default)
    {
        var storedSolutions = new HashSet<Guid>();
        foreach (var solution in solutions)
        {
            if (!storedSolutions.Add(solution.Id)) continue;

            var idBuffer = solution.Id.ToByteArray();

            var previousStepBuffer = new byte[16 + stateSerializer.SerializedStepLength];
            if (solution.PreviousStep != null)
            {
                Array.Copy(solution.PreviousStep.Value.Solution.Id.ToByteArray(), previousStepBuffer, 16);
                Array.Copy(stateSerializer.SerializeStep(solution.PreviousStep.Value.Step), 0, previousStepBuffer, 16, stateSerializer.SerializedStepLength);
            }

            var stateBuffer = stateSerializer.SerializeState(solution.State);

            var totalLength = idBuffer.Length + previousStepBuffer.Length + stateBuffer.Length;
            await stream.WriteAsync(BitConverter.GetBytes(totalLength), token);
            await stream.WriteAsync(idBuffer, token);
            await stream.WriteAsync(previousStepBuffer, token);
            await stream.WriteAsync(stateBuffer, token);
            await stream.FlushAsync(token);
        }
    }

    public async IAsyncEnumerable<(Guid id, Guid parent, TStep step, TState state)> Deserialize(Stream stream, [EnumeratorCancellation] CancellationToken token = default)
    {
        while (stream.CanRead)
        {
            var lengthBuffer = new byte[4];
            await stream.ReadExactlyAsync(lengthBuffer, token);
            var length = BitConverter.ToInt32(lengthBuffer);
            var solutionBuffer = new byte[length];
            await stream.ReadExactlyAsync(solutionBuffer, token);
            var id = new Guid(new ArraySegment<byte>(solutionBuffer, 0, 16));
            var parent = new Guid(new ArraySegment<byte>(solutionBuffer, 16, 16));
            var step = stateSerializer.DeserializeStep(new ArraySegment<byte>(solutionBuffer, 32,
                stateSerializer.SerializedStepLength));
            var start = 32 + stateSerializer.SerializedStepLength;
            var stateLength = length - start;
            var state = stateSerializer.DeserializeState(new ArraySegment<byte>(solutionBuffer, start, stateLength));
            yield return (id, parent, step, state);
        }
    }
}