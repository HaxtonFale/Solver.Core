using System.IO.Abstractions;
using System.Runtime.Caching;
using Microsoft.Extensions.Logging;
using Solver.Core.IO;
using Solver.Core.Serialization;

namespace Solver.Core.Cache;

public class DiskCache<TState, TStep> : ISolutionCache<TState, TStep>, IDisposable, IAsyncDisposable
{
    private readonly SolutionSerializer<TState, TStep> _serializer;
    private readonly ILogger<DiskCache<TState, TStep>> _logger;
    private readonly Stream _cacheStream;

    private readonly DiskCachePositionIndex _positions;
    private readonly MemoryCache _memoryCache;

    public DiskCache(IFileSystem fileSystem, SolutionSerializer<TState, TStep> serializer,
        IFilenameGenerator filenameGenerator, ILogger<DiskCache<TState, TStep>> logger)
    {
        _serializer = serializer;
        _logger = logger;
        filenameGenerator.AddFilenamePrefix("cache");
        _positions = new DiskCachePositionIndex(fileSystem, filenameGenerator);
        var cachePath = filenameGenerator.GetFilename("cache");
        logger.LogDebug("Initializing cache at {Path}", cachePath);
        _cacheStream = fileSystem.File.Open(cachePath, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.Read);

        _memoryCache = MemoryCache.Default;
    }

    #region ISolutionCache<TState,TStep>

    public Solution<TState, TStep> GetSolution(Guid solutionId)
    {
        _logger.LogInformation("Fetching solution {Id:D}", solutionId);
        if (!_positions.TryGetValue(solutionId, out var position))
        {
            throw new KeyNotFoundException($"Solution ID {solutionId} not found");
        }

        if (_memoryCache.Contains(solutionId.ToString("N")))
        {
            _logger.LogTrace("Solution found in memory");
            return (_memoryCache[solutionId.ToString("N")] as Solution<TState, TStep>)!;
        }

        _cacheStream.Seek(position, SeekOrigin.Begin);
        _logger.LogTrace("Solution found at position {Position}", position);

        var solution = _serializer.DeserializeSolutionAsync(_cacheStream).Result;
        _memoryCache.Set(solution.Id.ToString("N"), solution, DateTimeOffset.Now.AddHours(1));
        return solution;
    }

    public void StoreSolution(Solution<TState, TStep> solution)
    {
        _logger.LogInformation("Storing solution {Id:D}", solution.Id);
        if (_positions.ContainsKey(solution.Id))
        {
            throw new ArgumentException("Solution is already remembered", nameof(solution));
        }

        _cacheStream.Seek(0, SeekOrigin.End);
        var position = _cacheStream.Position;
        _serializer.SerializeSolutionAsync(solution, _cacheStream).Wait();
        _logger.LogTrace("Solution stored at position {Position}", position);
        _positions.Add(solution.Id, position);

        _memoryCache.Set(solution.Id.ToString("N"), solution, DateTimeOffset.Now.AddHours(1));
    }

    #endregion

    #region IDisposable

    public void Dispose()
    {
        _cacheStream.Dispose();
        _memoryCache.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        await _cacheStream.DisposeAsync();
        _memoryCache.Dispose();
    }

    #endregion
}