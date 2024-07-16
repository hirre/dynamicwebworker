
using WebWorker.Assembly;

namespace WebWorker.Worker
{
    public class AssemblyWorker : BackgroundService
    {
        private Guid _workerId;
        private ILogger<AssemblyWorker> _logger;
        private WebWorkerAssemblyLoadContext _webWorkerAssemblyLoadContext;
        private ManualResetEvent _messageEvent = new(false);

        public Guid Id => _workerId;

        public void SignalMessageEvent()
        {
            _messageEvent.Set();
        }

        public AssemblyWorker(Guid workerId, ILogger<AssemblyWorker> logger, WebWorkerAssemblyLoadContext webWorkerAssemblyLoadContext)
        {
            _workerId = workerId;
            _logger = logger;
            _webWorkerAssemblyLoadContext = webWorkerAssemblyLoadContext;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _messageEvent.WaitOne();

                    // TODO: Implement the worker logic here

                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Error in AssemblyWorker");
                }
            }
        }
    }
}
