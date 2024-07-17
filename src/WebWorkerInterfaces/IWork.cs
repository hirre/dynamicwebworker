namespace WebWorkerInterfaces
{
    public interface IWork
    {
        Task ExecuteWork(CancellationToken stoppingToken);
    }
}
