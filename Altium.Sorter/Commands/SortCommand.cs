namespace Altium.Sorter.Commands;

internal sealed class SortCommand : ICommand
{
    private readonly string _filePath;

    private SortCommand(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath)) 
            throw new ArgumentException("Invalid argument. Provide a file path.");
        _filePath = filePath;
    }

    public Task InvokeAsync(IProgress<ProgressReport> progress, CancellationToken cancellationToken)
    {
        new FileSorter().Sort(_filePath, progress, cancellationToken);
        return Task.CompletedTask;
    }

    internal static SortCommand Create(string[] args)
    {
        ArgumentNullException.ThrowIfNull(args);
        if (args.Length < 2)
            throw new ArgumentException("Invalid arguments. Provide a file path.");
        return new SortCommand(args[1]);
    }
}