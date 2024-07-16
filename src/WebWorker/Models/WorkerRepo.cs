using System.Collections.Concurrent;

namespace WebWorker.Models
{
    public class WorkerRepo
    {
        private ConcurrentDictionary<Guid, WorkerInfo> _workerInfos = new();

        public void AddWorker(WorkerInfo workerInfo)
        {
            _workerInfos.TryAdd(workerInfo.AssemblyWorker.Id, workerInfo);
        }

        public bool ContainsWorker(Guid workerId)
        {
            return _workerInfos.ContainsKey(workerId);
        }

        public WorkerInfo? GetWorker(Guid workerId)
        {
            _workerInfos.TryGetValue(workerId, out var workerInfo);
            return workerInfo;
        }

        public void RemoveWorker(Guid workerId)
        {
            _workerInfos.TryRemove(workerId, out _);
        }
    }
}
