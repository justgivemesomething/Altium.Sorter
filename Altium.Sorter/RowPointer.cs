namespace Altium.Sorter;

/// <summary>
/// Contains current row data and can read the next row. 
/// </summary>
internal struct RowPointer : IDisposable
{
    private readonly string _filePath;
    private readonly StreamReader _streamReader;
    
    internal RowPointer(string filePath)
    {
        _filePath = filePath;
        _streamReader = new StreamReader(filePath);
        Data = null;
    }
        
    public string? Data { get; private set; }

    internal string? ReadNext()
    {
        if (_streamReader.ReadLine() is not { } nextRow) return null;
        Data = nextRow;
        return nextRow;
    }

    public void Dispose()
    {
        _streamReader.Dispose();
        File.Delete(_filePath);
    }
}