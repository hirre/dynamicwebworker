using RabbitMQ.Client;
using System.Collections.Concurrent;
using WebWorker.Worker;

namespace WebWorker.Models
{
    /// <summary>
    ///     The worker repository.
    /// </summary>
    public class WorkerRepo
    {
        private ConcurrentDictionary<string, WorkerJob> _workerJobs = new();
        private ConcurrentDictionary<string, IModel> _channels = new();

        public void AddWorkerJob(WorkerJob workerJob)
        {
            _workerJobs.TryAdd(workerJob.Id, workerJob);
        }

        public void AddChannel(string id, IModel channel)
        {
            _channels.TryAdd(id, channel);
        }

        public bool ContainsWorkerJob(string id)
        {
            return _workerJobs.ContainsKey(id);
        }

        public WorkerJob? GetWorkerJob(string workerJob)
        {
            _workerJobs.TryGetValue(workerJob, out var wj);
            return wj;
        }

        public IModel? GetChannel(string id)
        {
            _channels.TryGetValue(id, out var channel);
            return channel;
        }

        public void RemoveWorkerJob(string id)
        {
            _workerJobs.TryRemove(id, out _);
            _channels.TryRemove(id, out _);
        }

        public WorkerJob[] GetWorkerJobArray()
        {
            return [.. _workerJobs.Values];
        }

        public IModel[] GetChannelArray()
        {
            return [.. _channels.Values];
        }

        public int GetWorkerJobCount()
        {
            return _workerJobs.Count;
        }

        public int GetChannelCount()
        {
            return _channels.Count;
        }
    }
}
