
using System.Collections.Concurrent;
using WebWorker.Assembly;
using WebWorker.Models;

namespace WebWorker.Worker
{
    public class WorkerJob : BackgroundService
    {
        private Guid _workerId;
        private ILogger<WorkerJob> _logger;
        private WebWorkerAssemblyLoadContext _webWorkerAssemblyLoadContext;
        private ManualResetEvent _messageEvent = new(false);
        private ConcurrentQueue<IMessage> _messageQueue = new();

        public Guid Id => _workerId;

        public WorkerJob(Guid workerId, ILogger<WorkerJob> logger, WebWorkerAssemblyLoadContext webWorkerAssemblyLoadContext)
        {
            _workerId = workerId;
            _logger = logger;
            _webWorkerAssemblyLoadContext = webWorkerAssemblyLoadContext;
        }
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

                        // TODO: Implement the worker logic here (process the message)


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
