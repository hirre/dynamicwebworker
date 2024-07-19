using System.Collections.Concurrent;

namespace WebWorker.Models
{
    public class WorkerRepo
    {
        private ConcurrentDictionary<string, WorkerData> _workerData = new();

        public void AddWorkerData(WorkerData workerInfo)
        {
            _workerData.TryAdd(workerInfo.Worker.Id, workerInfo);
        }

        public bool ContainsWorkerData(string workerId)
        {
            return _workerData.ContainsKey(workerId);
        }

        public WorkerData? GetWorkerData(string workerId)
        {
            _workerData.TryGetValue(workerId, out var workerInfo);
            return workerInfo;
        }

        public void RemoveWorkerData(string workerId)
        {
            _workerData.TryRemove(workerId, out _);
        }

        public WorkerData[] GetWorkerDataArray()
        {
            return [.. _workerData.Values];
        }

        public int GetWorkerDataCount()
        {
            return _workerData.Count;
        }
    }
}
