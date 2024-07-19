using RabbitMQ.Client;
using WebWorker.Models;

namespace WebWorker.MessageBroker
{
    public class RabbitMQConnectionService : IDisposable
    {
        private const int DEFAULT_PORT = 5672;

        private readonly IConfiguration _configuration;
        private readonly WorkerRepo _workerRepo;

        public required IConnection _connection;

        public RabbitMQConnectionService(IConfiguration configuration, WorkerRepo workerRepo)
        {
            _configuration = configuration;
            _workerRepo = workerRepo;

            InitializeRabbitMQ();
        }

        private void InitializeRabbitMQ()
        {
            var factory = new ConnectionFactory()
            {
                HostName = _configuration["RabbitMQ:HostName"],
                UserName = _configuration["RabbitMQ:UserName"],
                Password = _configuration["RabbitMQ:Password"],
                Port = int.TryParse(_configuration["RabbitMQ:Port"], out var port) ? port : DEFAULT_PORT,
                ClientProvidedName = "WebWorker",
                AutomaticRecoveryEnabled = bool.TryParse(_configuration["RabbitMQ:AutomaticRecoveryEnabled"], out var areVal) && areVal
            };

            _connection = factory.CreateConnection();
        }

        public IConnection GetConnection() => _connection;

        public void Dispose()
        {
            foreach (var workerInfo in _workerRepo.GetWorkerDataArray())
            {
                workerInfo.GetChannel?.Close();
            }

            _connection?.Close();
        }
    }
}
