using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using WebWorker.Assembly;
using WebWorker.MessageBroker;
using WebWorker.Models;
using WebWorker.Worker;

namespace WebWorker.Logic
{
    public class WorkLogic(IServiceProvider serviceProvider,
        RabbitMQConnectionService rabbitMQConnectionService,
        WorkerRepo workerRepo,
        ILogger<WorkLogic> logger)
    {
        private readonly IServiceProvider _serviceProvider = serviceProvider;
        private readonly RabbitMQConnectionService _rabbitMQConnectionService = rabbitMQConnectionService;
        private readonly WorkerRepo _workerRepo = workerRepo;
        private readonly ILogger<WorkLogic> _logger = logger;

        public void CreateWorker(CreateWorkerRequestDto createWorkerRequestDto)
        {
            if (_workerRepo.ContainsWorkerData(createWorkerRequestDto.WorkerId))
                throw new DuplicateWaitObjectException($"Worker {createWorkerRequestDto.WorkerId} already exists.");

            var workerService = new WorkerJob(createWorkerRequestDto.WorkerId, _serviceProvider.GetRequiredService<ILogger<WorkerJob>>(),
                        _serviceProvider.GetRequiredService<WebWorkerAssemblyLoadContext>());

            var conn = _rabbitMQConnectionService.GetConnection();
            var channel = conn.CreateModel();
            var queueName = createWorkerRequestDto.WorkerId;
            var exchangeName = "exchange." + queueName;
            var routingKey = "route." + exchangeName;

            channel.ExchangeDeclare(exchangeName, ExchangeType.Direct);

            channel.QueueDeclare(queue: queueName,
                                 durable: true,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

            channel.QueueBind(queueName, exchangeName, routingKey, null);

            var consumer = new AsyncEventingBasicConsumer(channel);

            consumer.Received += Consumer_Received;

            var wd = new WorkerData(workerService, channel, new CancellationTokenSource());

            _workerRepo.AddWorkerData(wd);

            _ = workerService.StartAsync(CancellationToken.None);

            channel.BasicConsume(queue: queueName,
                                 autoAck: false,
                                 consumer: consumer);
        }

        public void RemoveWorker(string id)
        {
            if (!_workerRepo.ContainsWorkerData(id))
                throw new KeyNotFoundException($"Worker {id} not found.");

            var workerInfo = _workerRepo.GetWorkerData(id) ?? throw new NullReferenceException($"Null worker {id}.");

            workerInfo.Worker.StopAsync(workerInfo.CancellationTokenSource.Token);
        }

        private async Task Consumer_Received(object sender, BasicDeliverEventArgs ea)
        {
            var body = ea.Body.ToArray();

            var msgJsonStr = Encoding.UTF8.GetString(body);

            var msg = JsonSerializer.Deserialize<IMessage>(msgJsonStr);

            if (msg != null)
            {
                var workerData = _workerRepo.GetWorkerData(msg.Id);

                workerData?.Worker.SignalMessageEvent(msg);
                workerData?.GetChannel.BasicAck(ea.DeliveryTag, false);
            }

            await Task.Yield();
        }
    }
}
