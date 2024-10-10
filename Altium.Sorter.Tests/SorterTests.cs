using FluentAssertions;
using NUnit.Framework;

namespace Altium.Sorter.Tests;

public class SorterTests
{
    private FileSorter FileSorter => new();

    private FileGenerator FileGenerator => new();
    
    private static CancellationToken CancellationToken => CancellationToken.None;

    private static Progress<ProgressReport> Progress => new();

    private static Random Random = new Random();

    private readonly List<string> _files = new();
        
    [Test]
    public void Sort_FileDoesNotExists_Throws()
    {
        Assert.Throws<FileNotFoundException>(() => FileSorter.Sort(Guid.NewGuid().ToString(), Progress, CancellationToken));
    }
    
    [Test]
    public void Sort_EmptyFile_Ok()
    {
        var filePath = Guid.NewGuid().ToString();
        File.Create(filePath).Dispose();
        _files.Add(filePath);
        Assert.DoesNotThrow(() => FileSorter.Sort(filePath, Progress, CancellationToken));
    }

    [Test]
    public void Sort_RowsAmountInSortedFile_Ok()
    {
        var filePath = Guid.NewGuid().ToString();
        var rowsCount = Random.Next(100, 20000);
        FileGenerator.Generate(filePath, rowsCount, Progress, CancellationToken);
        _files.Add(filePath);
        FileSorter.Sort(filePath, Progress, CancellationToken);
        var rows = File.ReadAllLines(filePath);
        rows.Length.Should().Be(rowsCount);
    }
    
    [Test]
    public void Sort_NotEmptyFile_SortedOk()
    {
        var filePath = Guid.NewGuid().ToString();
        File.WriteAllLines(filePath, new[]
        {
            "415. Apple",
            "30432. Something something something",
            "1. Apple",
            "32. Cherry is the best",
            "2. Banana is yellow"
        });
        _files.Add(filePath);

        FileSorter.Sort(filePath, Progress, CancellationToken);

        var rows = File.ReadAllLines(filePath);
        rows.Should().BeEquivalentTo("1. Apple", "415. Apple", "2. Banana is yellow", "32. Cherry is the best", "30432. Something something something");
    }

    [TearDown]
    public void OnTearDown()
    {
        foreach (var file in _files)
        {
            File.Delete(file);
        }
    }
}