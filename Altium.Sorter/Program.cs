using Altium.Sorter.Commands;

namespace Altium.Sorter;

internal sealed class Program
{
    private static readonly CancellationTokenSource Cts = new();
    
    static async Task Main(string[] args)
    {
        Console.CancelKeyPress += (_, eventArgs) =>
        {
            Cts.Cancel();
            eventArgs.Cancel = true;
        };
        
        var command = ParseCommand(args);
        var progress = new Progress<ProgressReport>(value => Console.Write("\r{0}: {1}%", value.OperationName, value.Progress));
        try
        {
            await command.InvokeAsync(progress, Cts.Token);
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("\nThe operation was canceled.");
        }
    }

    static ICommand ParseCommand(string[] args)
    {
        ArgumentNullException.ThrowIfNull(args);
        if (args.Length < 2)
            throw new ArgumentException("Invalid arguments. Provide an utility name (possible values: gen, sort) and their arguments.");
        var command = args[0];
        return command switch
        {
            "gen" => GenerateCommand.Create(args),
            "sort" => SortCommand.Create(args),
            _ => throw new ArgumentOutOfRangeException(nameof(command),"Invalid arguments. Provide an utility name (possible values: gen, sort) and their arguments.")
        };
    }
}