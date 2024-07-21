using WebWorker.Models;

namespace WebWorkerInterfaces
{
    public interface IWork
    {
        Task ExecuteWork(IMessage message, CancellationToken stoppingToken);
    }
}
