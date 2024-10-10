using BenchmarkDotNet.Running;

namespace Altium.Sorter.Benchmarks;

internal sealed class Program
{
    static void Main(string[] args)
    {
        BenchmarkRunner.Run<SorterBenchmark>();
    }
}