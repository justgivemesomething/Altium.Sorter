using System.Runtime.CompilerServices;

namespace Altium.Sorter;

/// <summary>
/// Provides a method to generate a file in a specific 'Number. String' format.
/// </summary>
internal sealed class FileGenerator
{
    private const int UniquenessPercentage = 80;
    private const int MaxRowLength = 1024;
    private const int MaxStringPoolLength = 10000;
    private static readonly Random Random = new();

    internal void Generate(string filePath, int rowsCount, IProgress<ProgressReport> progress, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentNullException(nameof(filePath));
        
        using var sw = new StreamWriter(filePath);
        var uniqueRowsCount = rowsCount * UniquenessPercentage / 100;
        var stringPoolLength = Math.Min(rowsCount - uniqueRowsCount, MaxStringPoolLength);
        // let's cache some amount of strings for 'non-uniqueness' purposes
        var stringsPool = new List<string>(stringPoolLength);
        var progressSegment = rowsCount / 10;
        
        for (var i = 1; i <= rowsCount; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            
            var nextNumber = Random.Next(0, uniqueRowsCount);
            if (ShouldBeUniqueString() || stringsPool.Count == 0)
            {
                var nextString = GetRandomString();
                if (stringsPool.Count < stringPoolLength)
                    stringsPool.Add(nextString);
                sw.WriteLine($"{nextNumber}. {nextString}");
            }
            else
            {
                var poolNumber = Random.Next(0, stringsPool.Count);
                sw.WriteLine($"{nextNumber}. {stringsPool[poolNumber]}");
            }
            
            if (i % progressSegment == 0 || i == rowsCount - 1)
                progress.Report(new ProgressReport("Generating file", i/progressSegment*10));
        }
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string GetRandomString()
    {
        var length = Random.Next(10, MaxRowLength + 1);
        return new string(Enumerable.Range(0, length).Select(_ => (char)Random.Next(65, 122)).ToArray());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool ShouldBeUniqueString()
    {
        var random = Random.Next(0, 100);
        if (random > UniquenessPercentage) return false;
        return true;
    }
}