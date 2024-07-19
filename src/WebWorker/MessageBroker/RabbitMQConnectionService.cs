using RabbitMQ.Client;
using WebWorker.Models;

namespace WebWorker.MessageBroker
{
    public class RabbitMQConnectionService : IDisposable
    {
        #region Configuration keys

        private const int DEFAULT_PORT = 5672;
        private const string RABBITMQ_HOSTNAME = "RabbitMQ:HostName";
        private const string RABBITMQ_USERNAME = "RabbitMQ:UserName";
        private const string RABBITMQ_PASSWORD = "RabbitMQ:Password";
        private const string RABBITMQ_PORT = "RabbitMQ:Port";
        private const string RABBITMQ_AUTORECOVERY = "RabbitMQ:AutomaticRecoveryEnabled";

        #endregion

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
                HostName = _configuration[RABBITMQ_HOSTNAME],
                UserName = _configuration[RABBITMQ_USERNAME],
                Password = _configuration[RABBITMQ_PASSWORD],
                Port = int.TryParse(_configuration[RABBITMQ_PORT], out var port) ? port : DEFAULT_PORT,
                ClientProvidedName = "WebWorker",
                AutomaticRecoveryEnabled = bool.TryParse(_configuration[RABBITMQ_AUTORECOVERY], out var areVal) && areVal
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
