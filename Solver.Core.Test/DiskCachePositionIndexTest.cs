using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using Solver.Core.Cache;
using Solver.Core.IO;
// ReSharper disable CollectionNeverQueried.Local

namespace Solver.Core.Test;

public class DiskCachePositionIndexTest
{
    private const string Filename = @"C:\index.bin";
    private readonly IFilenameGenerator _filenameGenerator = Substitute.For<IFilenameGenerator>();

    [Fact]
    public void WritesToDiskCorrectly()
    {
        // Arrange
        _filenameGenerator.GetFilename(Arg.Any<string>()).Returns(Filename);
        IFileSystem fileSystem = new MockFileSystem();
        var index = new DiskCachePositionIndex(fileSystem, _filenameGenerator);
        var id = Guid.NewGuid();
        var position = Random.Shared.NextInt64();

        // Act
        index.Add(id, position);

        // Assert
        index.Should().HaveCount(1).And.ContainKey(id).WhoseValue.Should().Be(position);
        var bytes = fileSystem.File.ReadAllBytes(Filename);
        var actualId = new Guid(new ArraySegment<byte>(bytes, 0, 16));
        actualId.Should().Be(id);
        var actualPosition = BitConverter.ToInt64(bytes, 16);
        actualPosition.Should().Be(position);
    }

    [Fact]
    public void AppendsToFile()
    {
        // Arrange
        _filenameGenerator.GetFilename(Arg.Any<string>()).Returns(Filename);
        IFileSystem fileSystem = new MockFileSystem();
        var index = new DiskCachePositionIndex(fileSystem, _filenameGenerator);
        var id1 = Guid.NewGuid();
        var position1 = Random.Shared.NextInt64();
        var id2 = Guid.NewGuid();
        var position2 = Random.Shared.NextInt64();
        var expected = new Dictionary<Guid, long>
        {
            { id1, position1 }, { id2, position2 }
        };

        // Act
        index.Add(id1, position1);
        index.Add(id2, position2);

        // Assert
        index.Should().BeEquivalentTo(expected);
        var bytes = fileSystem.File.ReadAllBytes(Filename);

        var actualId1 = new Guid(new ArraySegment<byte>(bytes, 0, 16));
        actualId1.Should().Be(id1);
        var actualPosition1 = BitConverter.ToInt64(bytes, 16);
        actualPosition1.Should().Be(position1);

        var actualId2 = new Guid(new ArraySegment<byte>(bytes, 16 + sizeof(long), 16));
        actualId2.Should().Be(id2);
        var actualPosition2 = BitConverter.ToInt64(bytes, 32 + sizeof(long));
        actualPosition2.Should().Be(position2);
    }

    [Fact]
    public void ThrowsExceptionIfFileExists()
    {
        // Arrange
        _filenameGenerator.GetFilename(Arg.Any<string>()).Returns(Filename);
        IFileSystem fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            {Filename, new MockFileData([])}
        });
        var act = () => new DiskCachePositionIndex(fileSystem, _filenameGenerator);

        // Act & Assert
        act.Should().Throw<IOException>();
    }

    [Fact]
    public void ThrowsExceptionOnDuplicateInsertion()
    {
        // Arrange
        _filenameGenerator.GetFilename(Arg.Any<string>()).Returns(Filename);
        IFileSystem fileSystem = new MockFileSystem();
        var index = new DiskCachePositionIndex(fileSystem, _filenameGenerator);
        var id = Guid.NewGuid();
        var position = Random.Shared.NextInt64();
        index.Add(id, position);
        var act = () => index.Add(id, position);

        // Act & Assert
        act.Should().Throw<InvalidOperationException>().WithMessage("Position already stored.");
    }
}