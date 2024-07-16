using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using WebWorker.Assembly;
using WebWorker.MessageBroker;
using WebWorker.Models;
using WebWorker.Worker;

namespace WebWorker.Logic
{
    public class WorkLogic
    {
        private IServiceProvider _serviceProvider;
        private RabbitMQConnectionService _rabbitMQConnectionService;
        private WorkerRepo _workerRepo;

        public WorkLogic(IServiceProvider serviceProvider,
            RabbitMQConnectionService rabbitMQConnectionService,
            WorkerRepo workerRepo)
        {
            _serviceProvider = serviceProvider;
            _rabbitMQConnectionService = rabbitMQConnectionService;
            _workerRepo = workerRepo;
        }

        public void CreateWorker(CreateWorkerRequestDto createWorkerRequestDto)
        {
            if (_workerRepo.ContainsWorker(createWorkerRequestDto.WorkerId))
                throw new DuplicateWaitObjectException($"Worker {createWorkerRequestDto.WorkerId} already exists.");

            // TODO: change T object to a specific message type
            var workerService = new WorkerJob(createWorkerRequestDto.WorkerId, _serviceProvider.GetRequiredService<ILogger<WorkerJob>>(),
                        _serviceProvider.GetRequiredService<WebWorkerAssemblyLoadContext>());

            var conn = _rabbitMQConnectionService.GetConnection();
            var channel = conn.CreateModel();
            var queueName = "" + createWorkerRequestDto.WorkerId;
            var exchangeName = createWorkerRequestDto.WorkerIdPrefix + "." + queueName;
            var routingKey = "route." + exchangeName;

            channel.ExchangeDeclare(exchangeName, ExchangeType.Direct);
            channel.QueueDeclare(queue: queueName,
                                 durable: true,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);
            channel.QueueBind(queueName, exchangeName, routingKey, null);


            var consumer = new EventingBasicConsumer(channel);

            consumer.Received += (model, ea) =>
            {

            };

            channel.BasicConsume(queue: queueName,
                                 autoAck: false,

                                 consumer: consumer);

            var wi = new WorkerInfo(workerService, channel, new CancellationTokenSource());

            _workerRepo.AddWorker(wi);

            _ = workerService.StartAsync(CancellationToken.None);
        }

        public void RemoveWorker(Guid id)
        {
            if (!_workerRepo.ContainsWorker(id))
                throw new KeyNotFoundException($"Worker {id} not found.");

            var workerInfo = _workerRepo.GetWorker(id);

            if (workerInfo == null)
                throw new NullReferenceException($"Null worker {id}.");

            workerInfo.AssemblyWorker.StopAsync(workerInfo.CancellationTokenSource.Token);
        }
    }
}
