using BenchmarkDotNet.Attributes;

namespace Altium.Sorter.Benchmarks;

[MemoryDiagnoser]
public class SorterBenchmark
{
    private const string FilePath = "benchmark.txt";
    
    private readonly FileSorter _fileSorter = new();
    private static Progress<ProgressReport> Progress => new ();

    [GlobalSetup]
    public void OnSetup()
    {
        new FileGenerator().Generate(FilePath, 1000000, Progress, CancellationToken.None);
    }

    [Benchmark]
    public void Sorter()
    {
        _fileSorter.Sort(FilePath, Progress, CancellationToken.None);
    }

    [GlobalCleanup]
    public void OnTearDown()
    {
        File.Delete(FilePath);
    }
}