using RabbitMQ.Client;
using WebWorker.Worker;

namespace WebWorker.Models
{
    public class WorkerData(WorkerJob worker, IModel channel)
    {
        public IModel GetChannel => channel;

        public WorkerJob Worker { get; } = worker;
    }
}
