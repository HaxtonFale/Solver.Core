using System.IO.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Solver.Core.Cache;

namespace Solver.Core.IO;

public class FilenameGenerator(IFileSystem fileSystem, IOptions<DiskCacheOptions> options, ILogger<FilenameGenerator> logger) : IFilenameGenerator
{
    private readonly HashSet<string> _prefixes = new();
    private readonly DateTimeOffset _date = DateTimeOffset.Now;
    private readonly string _root = options.Value.Root != null
        ? Environment.ExpandEnvironmentVariables(options.Value.Root)
        : fileSystem.Directory.GetCurrentDirectory();
    private int _counter;

    #region Implementation of IFilenameGenerator

    public void AddFilenamePrefix(string prefix)
    {
        logger.LogTrace("Registered filename prefix {Prefix}", prefix);
        _prefixes.Add(prefix);
    }

    public string GetFilename(string prefix)
    {
        if (!_prefixes.Contains(prefix)) throw new InvalidOperationException($"Prefix {prefix} not registered.");
        InitializeCounter();

        return FormatFilename(prefix);
    }

    #endregion

    private void InitializeCounter()
    {
        if (_counter != 0) return;
        while (_counter++ < int.MaxValue)
        {
            var any = false;
            foreach (var path in _prefixes.Select(FormatFilename))
            {
                logger.LogTrace("Testing path: {Path}", path);
                if (fileSystem.File.Exists(path))
                {
                    logger.LogTrace("File exists! Advancing counter.");
                    any = true;
                    break;
                }
            }

            if (!any) break;
        }
    }

    private string FormatFilename(string prefix)
        => Path.Combine(_root, $"{prefix}-{_date:yyyy-MM-dd}-{_counter:000}.bin");
}