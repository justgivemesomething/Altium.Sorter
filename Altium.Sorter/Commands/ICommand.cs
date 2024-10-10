namespace Altium.Sorter.Commands;

internal interface ICommand
{
    Task InvokeAsync(IProgress<ProgressReport> progress, CancellationToken cancellationToken);
}