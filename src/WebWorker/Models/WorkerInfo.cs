using WebWorker.Worker;

namespace WebWorker.Models
{
    public class WorkerInfo(AssemblyWorker assemblyWorker, CancellationTokenSource cancellationToken)
    {
        public CancellationTokenSource CancellationTokenSource { get; } = cancellationToken;

        public AssemblyWorker AssemblyWorker { get; } = assemblyWorker;
    }
}
