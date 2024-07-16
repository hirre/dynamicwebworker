
using System.Collections.Concurrent;
using WebWorker.Assembly;
using WebWorker.Models;

namespace WebWorker.Worker
{
    public class WorkerJob(string workerId, ILogger<WorkerJob> logger, WebWorkerAssemblyLoadContext webWorkerAssemblyLoadContext) : BackgroundService
    {
        private readonly ILogger<WorkerJob> _logger = logger;
        private readonly WebWorkerAssemblyLoadContext _webWorkerAssemblyLoadContext = webWorkerAssemblyLoadContext;
        private readonly ManualResetEvent _messageEvent = new(false);
        private readonly ConcurrentQueue<IMessage> _messageQueue = new();

        public string Id { get; } = workerId;

        public void SignalMessageEvent(IMessage message)
        {
            _messageQueue.Enqueue(message);
            _messageEvent.Set();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _messageEvent.WaitOne();

                    // Process messages while they exist
                    while (_messageQueue.TryDequeue(out var message))
                    {
                        if (stoppingToken.IsCancellationRequested)
                        {
                            break;
                        }

                        // TODO: call _webWorkerAssemblyLoadContext.LoadFromAssemblyPath(assemblyPath) to load the assembly

                    }

                    if (_messageQueue.IsEmpty)
                        _messageEvent.Reset();

                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Error in AssemblyWorker");
                }
            }
        }
    }
}
