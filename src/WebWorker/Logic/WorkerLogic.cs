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
    public class WorkerLogic(IConfiguration configuration,
        IServiceProvider serviceProvider,
        RabbitMQConnectionService rabbitMQConnectionService,
        WorkerRepo workerRepo,
        ILogger<WorkerLogic> logger)
    {
        #region Configuration keys



        #endregion

        private readonly IConfiguration _configuration = configuration;
        private readonly IServiceProvider _serviceProvider = serviceProvider;
        private readonly RabbitMQConnectionService _rabbitMQConnectionService = rabbitMQConnectionService;
        private readonly WorkerRepo _workerRepo = workerRepo;
        private readonly ILogger<WorkerLogic> _logger = logger;

        public async Task CreateWorker(CreateWorkerRequestDto createWorkerRequestDto)
        {
            var maxWorkers = int.TryParse(_configuration[Constants.WEBWORKER_MAX_WORKERS], out var maxWorkersVal) ? maxWorkersVal : 400;

            if (_workerRepo.GetWorkerDataCount() + 1 > maxWorkers)
                throw new InvalidOperationException("Maximum number of workers reached.");

            if (_workerRepo.ContainsWorkerData(createWorkerRequestDto.UniqueQueueId))
                throw new DuplicateWaitObjectException($"Worker {createWorkerRequestDto.UniqueQueueId} already exists.");

            var conn = _rabbitMQConnectionService.GetConnection();
            var channel = conn.CreateModel();
            var queueName = createWorkerRequestDto.UniqueQueueId;
            var exchangeName = "exchange." + queueName;
            var routingKey = "route." + queueName;

            var consumer = InitializeRabbitMQ(channel, exchangeName, queueName, routingKey);

            var useThreadPool = bool.TryParse(_configuration[Constants.WEBWORKER_USE_THREADPOOL], out var tPool) && tPool;

            if (!useThreadPool)
            {
                var workerService = new WorkerJob(routingKey, _serviceProvider.GetRequiredService<ILogger<WorkerJob>>(),
                            _serviceProvider.GetRequiredService<WebWorkerAssemblyLoadContext>(), new CancellationTokenSource());

                var wd = new WorkerData(workerService);

                _workerRepo.AddWorkerData(wd);

                workerService.Start();
            }

            _workerRepo.AddChannel(routingKey, channel);

            var autoAckValue = bool.TryParse(_configuration[Constants.RABBITMQ_QUEUE_AUTOACK], out var ackVal) && ackVal;

            channel.BasicConsume(queue: queueName,
                                 autoAck: autoAckValue,
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
                var useThreadPool = bool.TryParse(_configuration[Constants.WEBWORKER_USE_THREADPOOL], out var tPool) && tPool;
                var autoAckValue = bool.TryParse(_configuration[Constants.RABBITMQ_QUEUE_AUTOACK], out var ackVal) && ackVal;

                var workerData = _workerRepo.GetWorkerData(ea.RoutingKey);
                var channel = _workerRepo.GetChannel(ea.RoutingKey);

                if (useThreadPool)
                {
                    var threadPoolWorkHandler = new ThreadPoolWorkHandler();

                    Task.Run(() => threadPoolWorkHandler.Execute(msg));
                }
                else
                {
                    workerData?.Worker.SignalMessageEvent(msg);
                }

                if (!autoAckValue)
                    channel?.BasicAck(ea.DeliveryTag, false);
            }
        }

        private EventingBasicConsumer InitializeRabbitMQ(IModel channel, string exchangeName, string queueName, string routingKey)
        {
            var durableValue = bool.TryParse(_configuration[Constants.RABBITMQ_QUEUE_DURABLE], out var durVal) && durVal;
            var exclusiveValue = bool.TryParse(_configuration[Constants.RABBITMQ_QUEUE_EXCLUSIVE], out var exclVal) && exclVal;
            var autoDeleteValue = bool.TryParse(_configuration[Constants.RABBITMQ_QUEUE_AUTODELETE], out var autoDelVal) && autoDelVal;
            var exchangeType = string.IsNullOrEmpty(_configuration[Constants.RABBITMQ_CHANNEL_EXCHANGETYPE]) ?
                ExchangeType.Direct : _configuration[Constants.RABBITMQ_CHANNEL_EXCHANGETYPE];
            var channelQosPrefetchSize = uint.TryParse(_configuration[Constants.RABBITMQ_CHANNEL_QOS_PREFETCHSIZE], out var qosPrefetchSize) ?
                qosPrefetchSize : 0;
            var channelQosPrefetchCount = ushort.TryParse(_configuration[Constants.RABBITMQ_CHANNEL_QOS_PREFETCHCOUNT], out var qosPrefetchCount) ?
                qosPrefetchCount : (ushort)1;
            var channelQosGlobal = bool.TryParse(_configuration[Constants.RABBITMQ_CHANNEL_QOS_GLOBAL], out var qosGlobal) && qosGlobal;

            channel.ExchangeDeclare(exchangeName, exchangeType);

            channel.QueueDeclare(queue: queueName,
                                 durable: durableValue,
                                 exclusive: exclusiveValue,
                                 autoDelete: autoDeleteValue,
                                 arguments: LoadArguments(Constants.RABBITMQ_QUEUE_ARGUMENTS));

            channel.BasicQos(prefetchSize: channelQosPrefetchSize, prefetchCount: channelQosPrefetchCount, global: channelQosGlobal);

            channel.QueueBind(queueName, exchangeName, routingKey, LoadArguments(Constants.RABBITMQ_QUEUE_BIND_ARGUMENTS));

            var consumer = new EventingBasicConsumer(channel);

            consumer.Received += Consumer_Received;

            return consumer;
        }

        private Dictionary<string, object>? LoadArguments(string sectionName)
        {
            var argsSection = _configuration.GetSection(sectionName);

            var arguments = new Dictionary<string, object>();

            argsSection.GetChildren().AsEnumerable().ToList().ForEach(x =>
            {
                if (x.Value != null)
                {
                    if (bool.TryParse(x.Value, out bool boolValue))
                        arguments.Add(x.Key, boolValue);
                    else if (int.TryParse(x.Value, out int intValue))
                        arguments.Add(x.Key, intValue);
                    else if (double.TryParse(x.Value, out var doubleValue))
                        arguments.Add(x.Key, doubleValue);
                    else
                        arguments.Add(x.Key, x.Value);
                }
            });

            return arguments.Count != 0 ? arguments : null;
        }
    }
}
