
using WebWorker.Assembly;

namespace WebWorker.Worker
{
    public class AssemblyWorker : BackgroundService
    {
        ILogger<AssemblyWorker> _logger;
        WebWorkerAssemblyLoadContext _webWorkerAssemblyLoadContext;

        public AssemblyWorker(ILogger<AssemblyWorker> logger, WebWorkerAssemblyLoadContext webWorkerAssemblyLoadContext)
        {
            _logger = logger;
            _webWorkerAssemblyLoadContext = webWorkerAssemblyLoadContext;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Block, listen to RabbitMQ messages
                    await Task.Delay(3000, stoppingToken);

                    // Do RabbitMQ message processing by loading the corresponding assembly dynamically

                    Console.WriteLine("AssemblyWorker is doing background work.");

                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Error in AssemblyWorker");
                }
            }
        }
    }
}
