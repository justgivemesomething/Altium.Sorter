namespace Altium.Sorter.Commands;

internal sealed class GenerateCommand : ICommand
{
    private readonly int _rowsCount;
    private readonly string _filePath;
    
    private GenerateCommand(string rowsCount, string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath)) 
            throw new ArgumentException("Invalid argument. Provide a file path.");
        if (!int.TryParse(rowsCount, out var rows) || rows < 1)
            throw new ArgumentException("Invalid argument. Provide a positive number of rows to generate.");
        _rowsCount = rows;
        _filePath = filePath;
    }
    
    public Task InvokeAsync(IProgress<ProgressReport> progress, CancellationToken cancellationToken)
    {
        new FileGenerator().Generate(_filePath, _rowsCount, progress, cancellationToken);
        return Task.CompletedTask;
    }

    internal static GenerateCommand Create(string[] args)
    {
        ArgumentNullException.ThrowIfNull(args);
        if (args.Length < 3)
            throw new ArgumentException("Invalid arguments. Provide a number of rows to generate and a file path.");
        return new GenerateCommand(args[1], args[2]);
    }
}