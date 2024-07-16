using RabbitMQ.Client;
using WebWorker.Worker;

namespace WebWorker.Models
{
    public class WorkerData(WorkerJob worker, IModel channel, CancellationTokenSource cancellationToken)
    {
        public CancellationTokenSource CancellationTokenSource { get; } = cancellationToken;

        public IModel GetChannel => channel;

        public WorkerJob Worker { get; } = worker;
    }
}
