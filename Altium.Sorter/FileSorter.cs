using System.Runtime.CompilerServices;

namespace Altium.Sorter;

/// <summary>
/// Provides a method to sort large files not fitting in RAM. Split files into chunks and merges them into sorted one.
/// </summary>
internal sealed class FileSorter
{
    // Amount of rows in temp files, could be an optional value, but const for simplicity.
    private const int ChunkSize = 1024 * 10;

    private static readonly RowComparer DefaultRowComparer = new ();
    
    /// <summary>
    /// Sorts a file.
    /// </summary>
    /// <param name="filePath">Path to a file we want to sort.</param>
    /// <param name="progress">Provides a feedback about sorting progress.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <exception cref="ArgumentNullException">Throws if filePath is null or empty.</exception>
    /// <exception cref="FileNotFoundException">Throws if the file is not found.</exception>
    internal void Sort(string filePath, IProgress<ProgressReport> progress, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(filePath)) 
            throw new ArgumentNullException(nameof(filePath));
        if (!File.Exists(filePath)) 
            throw new FileNotFoundException("Provided file does not exist.");
        var (filesCount, totalRowsCount) = SplitFile(filePath, progress, cancellationToken);
        if (filesCount == 0) return;
        cancellationToken.ThrowIfCancellationRequested();
        MergeFiles(filePath, filesCount, totalRowsCount, progress, cancellationToken);
    }

    private static (int, int) SplitFile(string filePath, IProgress<ProgressReport> progress, CancellationToken cancellationToken)
    {
        var fileName = Path.GetFileNameWithoutExtension(filePath);
        var fileIndex = 0;
        var rowNumber = 0;
        var rows = new List<string>();
        const string operationName = "Splitting file";
        // can't really show progress here since the amount of rows is unknown, good thing to do it later:)
        progress.Report(new ProgressReport(operationName, 0));
        using var reader = new StreamReader(filePath);
        while (reader.ReadLine() is { } nextRow)
        {
            cancellationToken.ThrowIfCancellationRequested();
            // we have ChunkSize rows, let's write them into another file
            if (rowNumber % ChunkSize == 0 && rowNumber != 0)
            {
                var tempFileName = $"{fileName}_{fileIndex++}";
                WriteToTempFile(tempFileName, rows);
                rows = new List<string>();
            }

            rows.Add(nextRow);
            rowNumber++;
        }
        
        if (rows.Any())
        {
            var tempFileName = $"{fileName}_{fileIndex}";
            WriteToTempFile(tempFileName, rows);
        }
        
        progress.Report(new ProgressReport(operationName, 100));

        return (fileIndex, rowNumber);
    }
    
    private void MergeFiles(string filePath, int filesCount, int totalRowsCount, IProgress<ProgressReport> progress, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var fileName = Path.GetFileNameWithoutExtension(filePath);
        var progressSegment = totalRowsCount / 10;
        var sortedQueue = new PriorityQueue<RowPointer, string>(DefaultRowComparer);
        var rowsProgress = 0;
        
        for (var i = 0; i <= filesCount; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            // here we start reading every temp file, so we have N pointers
            var rowPointer = new RowPointer($"{fileName}_{i}");
            if (rowPointer.ReadNext() is { } nextRow)
                sortedQueue.Enqueue(rowPointer, nextRow);
        }
    
        using var writer = new StreamWriter(filePath);
        while (sortedQueue.Count > 0)
        {
            cancellationToken.ThrowIfCancellationRequested();
            // get a pointer with 'minimal' row, write it into the result file, 
            // then move the pointer to the next row.
            // All the sorting problem is taken care of by PriorityQueue.
            var rowPointer = sortedQueue.Dequeue();
            writer.WriteLine(rowPointer.Data);
            rowsProgress++;
            if (rowsProgress % progressSegment == 0 || rowsProgress == totalRowsCount)
                progress.Report(new ProgressReport("Merging large file", rowsProgress/progressSegment*10));
            
            if (rowPointer.ReadNext() is { } nextRow)
                sortedQueue.Enqueue(rowPointer, nextRow);
            else
                rowPointer.Dispose();
        }
    }
        
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void WriteToTempFile(string fileName, IEnumerable<string> rows)
    {
        using var sw = new StreamWriter(fileName);
        // just use OrderBy which is QuickSort
        foreach (var row in rows.OrderBy(l => l, DefaultRowComparer))
        {
            sw.WriteLine(row);
        }
    }
}