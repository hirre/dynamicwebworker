namespace WebWorkerInterfaces
{
    public interface IWork
    {
        Task ExecuteWork(object data, CancellationToken stoppingToken);
    }
}
