using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using WebWorker.Assembly;
using WebWorker.MessageBroker;
using WebWorker.Models;
using WebWorker.Worker;
using WebWorkerInterfaces;


namespace WebWorker.Logic
{
    public class WorkLogic(IConfiguration configuration,
        IServiceProvider serviceProvider,
        RabbitMQConnectionService rabbitMQConnectionService,
        WorkerRepo workerRepo,
        ILogger<WorkLogic> logger)
    {
        private readonly IConfiguration _configuration = configuration;
        private readonly IServiceProvider _serviceProvider = serviceProvider;
        private readonly RabbitMQConnectionService _rabbitMQConnectionService = rabbitMQConnectionService;
        private readonly WorkerRepo _workerRepo = workerRepo;
        private readonly ILogger<WorkLogic> _logger = logger;

        public async Task CreateWorker(CreateWorkerRequestDto createWorkerRequestDto)
        {
            if (_workerRepo.ContainsWorkerData(createWorkerRequestDto.WorkerId))
                throw new DuplicateWaitObjectException($"Worker {createWorkerRequestDto.WorkerId} already exists.");

            var workerService = new WorkerJob(createWorkerRequestDto.WorkerId, _serviceProvider.GetRequiredService<ILogger<WorkerJob>>(),
                        _serviceProvider.GetRequiredService<WebWorkerAssemblyLoadContext>(), new CancellationTokenSource());

            var conn = _rabbitMQConnectionService.GetConnection();
            var channel = conn.CreateModel();
            var queueName = createWorkerRequestDto.WorkerId;
            var exchangeName = "exchange." + queueName;
            var routingKey = "route." + queueName;

            channel.ExchangeDeclare(exchangeName, ExchangeType.Direct);

            // Declare the queue with the single active consumer argument
            var arguments = new Dictionary<string, object>
            {
                { "x-single-active-consumer", true }
            };

            channel.QueueDeclare(queue: queueName,
                                 durable: true,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: arguments);

            channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

            channel.QueueBind(queueName, exchangeName, routingKey, null);

            var consumer = new EventingBasicConsumer(channel);

            consumer.Received += Consumer_Received;

            var wd = new WorkerData(workerService, channel);

            _workerRepo.AddWorkerData(wd);

            workerService.Start();

            channel.BasicConsume(queue: queueName,
                                 autoAck: false,
                                 consumer: consumer);

            await Task.Yield();
        }

        public async Task RemoveWorker(string id)
        {
            if (!_workerRepo.ContainsWorkerData(id))
                throw new KeyNotFoundException($"Worker {id} not found.");

            var workerInfo = _workerRepo.GetWorkerData(id) ?? throw new NullReferenceException($"Null worker {id}.");

            await workerInfo.Worker.StopAsync();
        }

        private void Consumer_Received(object? sender, BasicDeliverEventArgs ea)
        {
            var body = ea.Body.ToArray();

            var msgJsonStr = Encoding.UTF8.GetString(body);

            var msg = JsonSerializer.Deserialize<TestMessage>(msgJsonStr);

            if (msg != null)
            {
                var workerData = _workerRepo.GetWorkerData(msg.Id);

                workerData?.Worker.SignalMessageEvent(msg);
                workerData?.GetChannel.BasicAck(ea.DeliveryTag, false);
            }
        }
    }
}
