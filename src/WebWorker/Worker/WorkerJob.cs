
using System.Collections.Concurrent;
using WebWorker.Assembly;
using WebWorker.Models;

namespace WebWorker.Worker
{
    public class WorkerJob(string id, ILogger<WorkerJob> logger, WebWorkerAssemblyLoadContext webWorkerAssemblyLoadContext,
        CancellationTokenSource cancellationTokenSource)
    {
#pragma warning disable CS9124 // Parameter is captured into the state of the enclosing type and its value is also used to initialize a field, property, or event.
        private readonly CancellationTokenSource _cancellationTokenSource = cancellationTokenSource;
#pragma warning restore CS9124 // Parameter is captured into the state of the enclosing type and its value is also used to initialize a field, property, or event.

        private readonly ILogger<WorkerJob> _logger = logger;
        private readonly WebWorkerAssemblyLoadContext _webWorkerAssemblyLoadContext = webWorkerAssemblyLoadContext;
        private readonly ManualResetEvent _messageEvent = new(false);
        private readonly ConcurrentQueue<IMessage> _messageQueue = new();
        private bool _jobCreated;

        public string Id { get; } = id;

        public void Start()
        {
            if (_jobCreated)
                return;

            Task.Factory.StartNew(() => ProcessWork(), _cancellationTokenSource.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
            _jobCreated = true;
        }

        public async Task StopAsync()
        {
            if (_cancellationTokenSource.IsCancellationRequested || !_jobCreated)
                return;

            await _cancellationTokenSource.CancelAsync();
        }

        public void SignalMessageEvent(IMessage message)
        {
            _messageQueue.Enqueue(message);
            _messageEvent.Set();
        }

        private void ProcessWork()
        {
            _logger.LogInformation($"Starting worker (# {Id})...");

            while (!_cancellationTokenSource.IsCancellationRequested)
            {
                try
                {
                    _messageEvent.WaitOne();

                    // Process messages while they exist
                    while (_messageQueue.TryDequeue(out var message))
                    {
                        if (cancellationTokenSource.IsCancellationRequested)
                        {
                            break;
                        }

                        _logger.LogInformation("Processing message...");
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
