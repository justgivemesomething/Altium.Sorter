namespace Altium.Sorter;

/// <summary>
/// Specific comparer for string in a 'Number. String' format, which firstly compares the strings, then the numbers if the strings are equal.
/// </summary>
internal class RowComparer : IComparer<string>
{
    private const char Dot = '.';
    
    public int Compare(string? x, string? y)
    {
        if (x == null && y == null) return 0;
        if (x != null && y == null) return 1;
        if (x == null && y != null) return -1;
        var xDotIndex = x!.IndexOf(Dot);
        var yDotIndex = y!.IndexOf(Dot);
        var row1 = x.AsSpan(xDotIndex + 2);
        var row2 = y.AsSpan(yDotIndex + 2);
        var textCompareResult = row1.CompareTo(row2, StringComparison.Ordinal);
        // if 0 - the strings are equal, need to compare the numbers
        if (textCompareResult != 0) return textCompareResult;

        // I expect file in a valid format
        var row1Number = int.Parse(x.AsSpan(0, xDotIndex));
        var row2Number = int.Parse(y.AsSpan(0, yDotIndex));
        return row1Number.CompareTo(row2Number);
    }
}