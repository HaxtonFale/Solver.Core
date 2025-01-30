using Microsoft.Extensions.Logging;

namespace Solver.Core.Serialization;

public class SolutionSerializer<TState, TStep>(IStateSerializer<TState, TStep> stateSerializer, ILogger<SolutionSerializer<TState, TStep>>? logger = null)
{

    public async Task SerializeSolutionAsync(Solution<TState, TStep> solution, Stream stream, CancellationToken token = default)
    {
        logger?.LogDebug("Serializing solution {Guid:D}", solution.Id);

        var idBuffer = solution.Id.ToByteArray();
        logger?.LogTrace("GUID bytes: {Bytes}", PrintBytes(idBuffer));

        var stepsCount = BitConverter.GetBytes(solution.Length);
        logger?.LogTrace("Solution steps count bytes: {Bytes}", PrintBytes(stepsCount));

        var previousStepBuffer = new byte[16 + stateSerializer.SerializedStepLength];
        if (solution.Previous != null)
        {
            Array.Copy(solution.Previous.Value.Id.ToByteArray(), previousStepBuffer, 16);
            var stepBuffer = new byte[stateSerializer.SerializedStepLength];
            stateSerializer.SerializeStep(solution.Previous.Value.Step, stepBuffer);
            Array.Copy(stepBuffer, 0, previousStepBuffer, 16, stateSerializer.SerializedStepLength);
        }
        else
        {
            Array.Fill(previousStepBuffer, byte.MinValue);
        }
        logger?.LogTrace("Step bytes: {Bytes} ({Length})", PrintBytes(previousStepBuffer), previousStepBuffer.Length);

        var stateBuffer = new byte[1024];
        var stateLength = stateSerializer.SerializeState(solution.State, stateBuffer);
        logger?.LogTrace("State length: {Length}", stateLength);

        var totalLength = idBuffer.Length + previousStepBuffer.Length + stepsCount.Length + stateLength;
        var lengthBytes = BitConverter.GetBytes(totalLength);
        logger?.LogTrace("Total length: {Length} ({Bytes})", totalLength, PrintBytes(lengthBytes));

        logger?.LogDebug("Writing bytes to file...");
        await stream.WriteAsync(lengthBytes, token);
        await stream.WriteAsync(idBuffer, token);
        await stream.WriteAsync(stepsCount, token);
        await stream.WriteAsync(previousStepBuffer, token);
        await stream.WriteAsync(stateBuffer.AsMemory(0, stateLength), token);
        await stream.FlushAsync(token);
    }

    public async Task<Solution<TState, TStep>> DeserializeSolutionAsync(Stream stream, CancellationToken token = default)
    {
        logger?.LogDebug("Deserializing solution...");
        logger?.LogTrace("Stream length: {Length}", stream.Length);
        logger?.LogTrace("Stream at byte {Byte} ({Byte:X8})", stream.Position, stream.Position);
        CheckStreamLength(4);
        var lengthBuffer = new byte[4];
        await stream.ReadExactlyAsync(lengthBuffer, 0, 4, token);
        logger?.LogTrace("Length bytes: {Bytes}", PrintBytes(lengthBuffer));
        var length = BitConverter.ToInt32(lengthBuffer);
        logger?.LogTrace("Read length: {Length} bytes", length);
        logger?.LogTrace("Stream at byte {Byte}", stream.Position);
        CheckStreamLength(length);
        var solutionBuffer = new byte[length];
        await stream.ReadExactlyAsync(solutionBuffer, 0, length, token);
        var idBytes = new ArraySegment<byte>(solutionBuffer, 0, 16);
        var id = new Guid(idBytes);
        logger?.LogTrace("Read ID: {Guid:D} ({Bytes})", id, PrintBytes(idBytes));
        var stepsCountBytes = new ArraySegment<byte>(solutionBuffer, 16, 4);
        var stepsCount = BitConverter.ToUInt32(stepsCountBytes);
        logger?.LogTrace("Read step count: {Length}", stepsCount);
        var parentBytes = new ArraySegment<byte>(solutionBuffer, 20, 16);
        var parent = new Guid(parentBytes);
        logger?.LogTrace("Parent: {Guid:D} ({Bytes})", parent, PrintBytes(parentBytes));
        var step = parent == Guid.Empty
            ? default
            : stateSerializer.DeserializeStep(new ArraySegment<byte>(solutionBuffer, 36,
                stateSerializer.SerializedStepLength));
        var start = 36 + stateSerializer.SerializedStepLength;
        var stateLength = length - start;
        var state = stateSerializer.DeserializeState(new ArraySegment<byte>(solutionBuffer, start,
            stateLength));

        return new Solution<TState, TStep>(state)
        {
            Id = id,
            Length = stepsCount,
            Previous = parent == Guid.Empty ? default : (parent, step!)
        };

        void CheckStreamLength(int readLength)
        {
            if (stream.Position + readLength > stream.Length)
            {
                throw new IOException("Input stream too short.");
            }
        }
    }

    private static string PrintBytes(byte[] bytes) => string.Join("", bytes.Select(b => b.ToString("X2")));
    private static string PrintBytes(ReadOnlySpan<byte> bytes) => PrintBytes(bytes.ToArray());
}