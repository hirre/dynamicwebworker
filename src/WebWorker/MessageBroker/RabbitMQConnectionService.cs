using RabbitMQ.Client;
using WebWorker.Models;

namespace WebWorker.MessageBroker
{
    public class RabbitMQConnectionService : IDisposable
    {
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
                HostName = _configuration[Constants.RABBITMQ_HOSTNAME],
                UserName = _configuration[Constants.RABBITMQ_USERNAME],
                Password = _configuration[Constants.RABBITMQ_PASSWORD],
                Port = int.TryParse(_configuration[Constants.RABBITMQ_PORT], out var port) ? port : Constants.RABBITMQ_DEFAULT_PORT,
                ClientProvidedName = "WebWorker",
                AutomaticRecoveryEnabled = bool.TryParse(_configuration[Constants.RABBITMQ_AUTORECOVERY], out var areVal) && areVal
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
