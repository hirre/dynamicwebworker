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

        public void AddChannel(string id, IModel channel)
        {
            _channels.TryAdd(id, channel);
        }

        public bool ContainsWorkerData(string id)
        {
            return _workerData.ContainsKey(id);
        }

        public WorkerData? GetWorkerData(string workerData)
        {
            _workerData.TryGetValue(workerData, out var wd);
            return wd;
        }

        public IModel? GetChannel(string id)
        {
            _channels.TryGetValue(id, out var channel);
            return channel;
        }

        public void RemoveWorkerData(string id)
        {
            _workerData.TryRemove(id, out _);
            _channels.TryRemove(id, out _);
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
