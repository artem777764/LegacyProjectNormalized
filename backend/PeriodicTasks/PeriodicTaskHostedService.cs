namespace backend.PeriodicTasks;

public class PeriodicTaskHostedService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly List<Task> _running = new();
    private readonly CancellationTokenSource _cts = new();

    public PeriodicTaskHostedService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        using (var scope = _serviceProvider.CreateScope())
        {
            var tasks = scope.ServiceProvider.GetServices<IPeriodicTask>().ToList();
            foreach (var task in tasks)
            {
                Console.WriteLine($"- {task.Name}, interval {task.Interval.TotalSeconds} sec");

                var type = task.GetType();
                var interval = task.Interval;

                _running.Add(Task.Run(() => RunLoopByTypeAsync(type, interval, _cts.Token)));
            }
        }

        return Task.CompletedTask;
    }

    private async Task RunLoopByTypeAsync(Type taskType, TimeSpan interval, CancellationToken token)
    {
        Console.WriteLine($"Task loop started for {taskType.Name}");

        while (!token.IsCancellationRequested)
        {
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var taskInstance = (IPeriodicTask)ActivatorUtilities.CreateInstance(scope.ServiceProvider, taskType);
                    await taskInstance.ExecuteAsync(token);
                }
            }
            catch (OperationCanceledException) when (token.IsCancellationRequested)
            {
                break;
            }

            try
            {
                await Task.Delay(interval, token);
            }
            catch (TaskCanceledException)
            {
                break;
            }
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _cts.Cancel();
        try
        {
            await Task.WhenAll(_running.ToArray()).WaitAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            
        }
    }
}
