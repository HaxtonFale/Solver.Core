using System.Collections;
using System.IO.Abstractions;
using Solver.Core.IO;
using NotSupportedException = System.NotSupportedException;

namespace Solver.Core.Cache;

public class DiskCachePositionIndex : IDictionary<Guid, long>, IDisposable, IAsyncDisposable
{
    private readonly Stream _fileStream;
    private readonly Dictionary<Guid, long> _positions = new();

    public DiskCachePositionIndex(IFileSystem fileSystem, IFilenameGenerator filenameGenerator)
    {
        filenameGenerator.AddFilenamePrefix("index");
        var indexCachePath = filenameGenerator.GetFilename("index");
        _fileStream = fileSystem.File.Open(indexCachePath, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.Read);
    }

    #region IDisposable

    public void Dispose()
    {
        _fileStream.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        await _fileStream.DisposeAsync();
    }

    #endregion

    #region Implementation of IEnumerable

    public IEnumerator<KeyValuePair<Guid, long>> GetEnumerator() => _positions.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    #endregion

    #region Implementation of ICollection<KeyValuePair<Guid,long>>

    public void Add(KeyValuePair<Guid, long> item) => Add(item.Key, item.Value);

    public void Clear() => throw new NotSupportedException();

    public bool Contains(KeyValuePair<Guid, long> item) => _positions.ContainsKey(item.Key) && _positions[item.Key] == item.Value;

    public void CopyTo(KeyValuePair<Guid, long>[] array, int arrayIndex)
    {
        var positionsArray = _positions.ToArray();
        positionsArray.CopyTo(array, arrayIndex);
    }

    public bool Remove(KeyValuePair<Guid, long> item) => throw new NotSupportedException();

    public int Count => _positions.Count;
    public bool IsReadOnly => false;

    #endregion

    #region Implementation of IDictionary<Guid,long>

    public void Add(Guid key, long value)
    {
        if (!_positions.TryAdd(key, value)) throw new InvalidOperationException("Position already stored.");
        _fileStream.Write(key.ToByteArray());
        _fileStream.Write(BitConverter.GetBytes(value));
        _fileStream.Flush();
    }

    public bool ContainsKey(Guid key) => _positions.ContainsKey(key);

    public bool Remove(Guid key) => throw new NotSupportedException();

    public bool TryGetValue(Guid key, out long value) => _positions.TryGetValue(key, out value);

    public long this[Guid key]
    {
        get => _positions[key];
        set => throw new NotSupportedException();
    }

    public ICollection<Guid> Keys => _positions.Keys;
    public ICollection<long> Values => _positions.Values;

    #endregion
}