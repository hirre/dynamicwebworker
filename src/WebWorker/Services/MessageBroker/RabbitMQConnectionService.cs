using RabbitMQ.Client;
using WebWorker.Models;

namespace WebWorker.Services.MessageBroker
{
    /// <summary>
    ///     The RabbitMQ service configuration.
    /// </summary>
    public class RabbitMQConnectionService : IDisposable
    {
        private readonly IConfiguration _configuration;
        private readonly WorkerRepo _workerRepo;

        public required IConnection _connection;

        /// <summary>
        ///     Creates a new instance of the RabbitMQ service.
        /// </summary>
        /// <param name="configuration">IConfiguration object</param>
        /// <param name="workerRepo">Worker repository object</param>
        public RabbitMQConnectionService(IConfiguration configuration, WorkerRepo workerRepo)
        {
            _configuration = configuration;
            _workerRepo = workerRepo;

            InitializeRabbitMQ();
        }

        /// <summary>
        ///     Initializes the RabbitMQ connection.
        /// </summary>
        private void InitializeRabbitMQ()
        {
            var factory = new ConnectionFactory()
            {
                HostName = _configuration[Definitions.RABBITMQ_HOSTNAME],
                UserName = _configuration[Definitions.RABBITMQ_USERNAME],
                Password = _configuration[Definitions.RABBITMQ_PASSWORD],
                Port = int.TryParse(_configuration[Definitions.RABBITMQ_PORT], out var port) ? port : Definitions.RABBITMQ_DEFAULT_PORT,
                ClientProvidedName = "WebWorker",
                AutomaticRecoveryEnabled = bool.TryParse(_configuration[Definitions.RABBITMQ_AUTORECOVERY], out var areVal) && areVal
            };

            _connection = factory.CreateConnection();
        }

        /// <summary>
        ///     Gets the RabbitMQ connection.
        /// </summary>
        /// <returns>IConnection object</returns>
        public IConnection GetConnection() => _connection;

        /// <summary>
        ///     Disposes the RabbitMQ connection.
        /// </summary>
        public void Dispose()
        {
            foreach (var channel in _workerRepo.GetChannelArray())
            {
                channel?.Close();
            }

            _connection?.Close();
        }
    }
}
