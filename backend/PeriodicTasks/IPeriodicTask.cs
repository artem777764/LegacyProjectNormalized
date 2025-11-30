namespace backend.PeriodicTasks;

public interface IPeriodicTask
{
    public string Name { get; }
    public TimeSpan Interval { get; }
    public Task ExecuteAsync(CancellationToken stoppingToken);
}