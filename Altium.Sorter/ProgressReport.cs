namespace Altium.Sorter;

public struct ProgressReport
{
    public ProgressReport(string operationName, int progress)
    {
        OperationName = operationName;
        Progress = progress;
    }

    public string OperationName { get; }
    
    public int Progress { get; }
}