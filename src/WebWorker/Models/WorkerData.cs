using WebWorker.Worker;

namespace WebWorker.Models
{
    public class WorkerData(WorkerJob worker)
    {
        public WorkerJob Worker { get; } = worker;
    }
}
