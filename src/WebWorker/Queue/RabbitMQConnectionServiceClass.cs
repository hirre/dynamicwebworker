using RabbitMQ.Client;
using WebWorker.Models;

namespace WebWorker.Queue
{
    public class RabbitMQConnectionService : IDisposable
    {
        private readonly IConfiguration _configuration;
        private WorkerRepo _workerRepo;

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
                ClientProvidedName = "WebWorker"
            };

            _connection = factory.CreateConnection();
        }

        public IConnection GetConnection() => _connection;

        public void Dispose()
        {
            foreach (var workerInfo in _workerRepo.GetWorkerInfos())
            {
                workerInfo.GetChannel?.Close();
            }

            _connection?.Close();
        }
    }
}
