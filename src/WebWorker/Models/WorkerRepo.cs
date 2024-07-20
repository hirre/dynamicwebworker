using RabbitMQ.Client;
using System.Collections.Concurrent;

namespace WebWorker.Models
{
    public class WorkerRepo
    {
        private ConcurrentDictionary<string, WorkerData> _workerData = new();
        private ConcurrentDictionary<string, IModel> _channels = new();

        public void AddWorkerData(WorkerData workerData)
        {
            _workerData.TryAdd(workerData.Worker.Id, workerData);
        }

        public void AddChannel(string workerId, IModel channel)
        {
            _channels.TryAdd(workerId, channel);
        }

        public bool ContainsWorkerData(string workerId)
        {
            return _workerData.ContainsKey(workerId);
        }

        public WorkerData? GetWorkerData(string workerData)
        {
            _workerData.TryGetValue(workerData, out var wd);
            return wd;
        }

        public IModel? GetChannel(string workerId)
        {
            _channels.TryGetValue(workerId, out var channel);
            return channel;
        }

        public void RemoveWorkerData(string workerId)
        {
            _workerData.TryRemove(workerId, out _);
            _channels.TryRemove(workerId, out _);
        }

        public WorkerData[] GetWorkerDataArray()
        {
            return [.. _workerData.Values];
        }

        public IModel[] GetChannelArray()
        {
            return [.. _channels.Values];
        }

        public int GetWorkerDataCount()
        {
            return _workerData.Count;
        }

        public int GetChannelCount()
        {
            return _channels.Count;
        }
    }
}
