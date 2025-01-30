namespace Solver.Core.IO;

public interface IFilenameGenerator
{
    public void AddFilenamePrefix(string prefix);
    public string GetFilename(string prefix);
}