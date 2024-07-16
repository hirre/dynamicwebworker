using RabbitMQ.Client;
using WebWorker.Worker;

namespace WebWorker.Models
{
    public class WorkerInfo(WorkerJob assemblyWorker, IModel channel, CancellationTokenSource cancellationToken)
    {
        public CancellationTokenSource CancellationTokenSource { get; } = cancellationToken;

        public IModel GetChannel => channel;

        public WorkerJob AssemblyWorker { get; } = assemblyWorker;
    }
}
