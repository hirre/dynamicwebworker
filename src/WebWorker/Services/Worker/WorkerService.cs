using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using WebWorker.Assembly;
using WebWorker.Models;
using WebWorker.Services.MessageBroker;
using WebWorkerInterfaces;

namespace WebWorker.Services.Worker
{
    /// <summary>
    ///     This class contains the logic for creating and removing workers.
    /// </summary>
    /// <param name="configuration">IConfiguration instance</param>
    /// <param name="serviceProvider">IServiceProvider instance</param>
    /// <param name="rabbitMQConnectionService">The RabbitMQ service provider</param>
    /// <param name="workerRepo">A repository of containing worker and RabbitMQ channel data</param>
    /// <param name="workPluginRepo">The work plugin repository</param>
    /// <param name="logger">The logger</param>
    public class WorkerService(IConfiguration configuration,
        IServiceProvider serviceProvider,
        RabbitMQConnectionService rabbitMQConnectionService,
        WorkerRepo workerRepo,
        WorkPluginRepo workPluginRepo,
        ILogger<WorkerService> logger)
    {
        private readonly IConfiguration _configuration = configuration;
        private readonly IServiceProvider _serviceProvider = serviceProvider;
        private readonly RabbitMQConnectionService _rabbitMQConnectionService = rabbitMQConnectionService;
        private readonly WorkerRepo _workerRepo = workerRepo;
        private readonly WorkPluginRepo _workPluginRepo = workPluginRepo;
        private readonly ILogger<WorkerService> _logger = logger;

        /// <summary>
        ///     Creates a worker.
        /// </summary>
        /// <param name="createWorkerRequestDto">Worker request</param>
        /// <returns>200 on success</returns>
        /// <exception cref="MaxWorkerLimitReachedException">If maximum worker limit is reached</exception>
        /// <exception cref="DuplicateWorkerException">If worker already exists</exception>
        public async Task<Result<string?>> CreateWorker(CreateWorkerRequestDto createWorkerRequestDto)
        {
            var maxWorkers = int.TryParse(_configuration[Definitions.WEBWORKER_MAX_WORKERS], out var maxWorkersVal) ? maxWorkersVal : 400;
            var queueName = createWorkerRequestDto.UniqueQueueId;
            var exchangeName = "exchange." + queueName;
            var routingKey = "route." + queueName;

            if (_workerRepo.GetWorkerJobCount() + 1 > maxWorkers)
                return Result<string>.Failure("Maximum number of workers reached", ErrorCodes.MaximumWorkersReached);

            if (_workerRepo.ContainsWorkerJob(routingKey))
                return Result<string>.Failure($"Worker {createWorkerRequestDto.UniqueQueueId} already exists", ErrorCodes.WorkerJobAlreadyExists);

            var conn = _rabbitMQConnectionService.GetConnection();
            var channel = conn.CreateModel();

            var consumer = InitializeRabbitMQ(channel, exchangeName, queueName, routingKey);

            var useThreadPool = bool.TryParse(_configuration[Definitions.WEBWORKER_USE_THREADPOOL], out var tPool) && tPool;

            if (!useThreadPool)
            {
                var wJob = new WorkerJob(routingKey, _serviceProvider.GetRequiredService<ILogger<WorkerJob>>(),
                            _serviceProvider.GetRequiredService<WorkPluginRepo>(), new CancellationTokenSource());

                _workerRepo.AddWorkerJob(wJob);

                wJob.Start();
            }

            _workerRepo.AddChannel(routingKey, channel);

            var autoAckValue = bool.TryParse(_configuration[Definitions.RABBITMQ_QUEUE_AUTOACK], out var ackVal) && ackVal;

            channel.BasicConsume(queue: queueName,
                                 autoAck: autoAckValue,
                                 consumer: consumer);

            await Task.Yield();

            return Result<string>.Success();
        }

        /// <summary>
        ///     Remove worker from the system.
        /// </summary>
        /// <param name="id">Worker id</param>
        /// <returns>Task</returns>
        /// <exception cref="WorkerNotFoundException">When the worker can't be found</exception>
        /// <exception cref="NullReferenceException">If the worker object is null</exception>
        public async Task<Result<string?>> RemoveWorker(string id)
        {
            if (!_workerRepo.ContainsWorkerJob(id))
                return Result<string>.Failure($"Worker {id} not found", ErrorCodes.WorkerJobNotFound);

            var workerJob = _workerRepo.GetWorkerJob(id) ?? throw new NullReferenceException($"Null worker {id}.");

            workerJob.Stop();

            _workerRepo.GetChannel(id)?.Close();

            await Task.Yield();

            return Result<string>.Success();
        }

        /// <summary>
        ///     Consumer event handler.
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="ea">Delivery arguments</param>
        private void Consumer_Received(object? sender, BasicDeliverEventArgs ea)
        {
            var body = ea.Body.ToArray();

            var msgJsonStr = Encoding.UTF8.GetString(body);

            var msg = JsonSerializer.Deserialize<WebWorkerMessage>(msgJsonStr);

            if (msg != null)
            {
                var useThreadPool = bool.TryParse(_configuration[Definitions.WEBWORKER_USE_THREADPOOL], out var tPool) && tPool;
                var autoAckValue = bool.TryParse(_configuration[Definitions.RABBITMQ_QUEUE_AUTOACK], out var ackVal) && ackVal;

                var workerJob = _workerRepo.GetWorkerJob(ea.RoutingKey);
                var channel = _workerRepo.GetChannel(ea.RoutingKey);

                if (useThreadPool)
                {
                    var work = _workPluginRepo.GetWorkPlugin(msg.WorkClassName);

                    if (work != null)
                        Task.Run(() => work.ExecuteWork(msg.Data, new CancellationTokenSource().Token).GetAwaiter().GetResult());
                    else
                        _logger.LogError($"Work plugin {msg.WorkClassName} not found.");

                }
                else
                {
                    workerJob?.SignalMessageEvent(msg);
                }

                if (!autoAckValue)
                    channel?.BasicAck(ea.DeliveryTag, false);
            }
        }

        /// <summary>
        ///     Initialize RabbitMQ.
        /// </summary>
        /// <param name="channel">Channel model</param>
        /// <param name="exchangeName">Exchange name</param>
        /// <param name="queueName">Queue name</param>
        /// <param name="routingKey">Routing key</param>
        /// <returns>EventingBasicConsumer object</returns>
        private EventingBasicConsumer InitializeRabbitMQ(IModel channel, string exchangeName, string queueName, string routingKey)
        {
            var durableValue = bool.TryParse(_configuration[Definitions.RABBITMQ_QUEUE_DURABLE], out var durVal) && durVal;
            var exclusiveValue = bool.TryParse(_configuration[Definitions.RABBITMQ_QUEUE_EXCLUSIVE], out var exclVal) && exclVal;
            var autoDeleteValue = bool.TryParse(_configuration[Definitions.RABBITMQ_QUEUE_AUTODELETE], out var autoDelVal) && autoDelVal;
            var exchangeType = string.IsNullOrEmpty(_configuration[Definitions.RABBITMQ_CHANNEL_EXCHANGETYPE]) ?
                ExchangeType.Direct : _configuration[Definitions.RABBITMQ_CHANNEL_EXCHANGETYPE];
            var channelQosPrefetchSize = uint.TryParse(_configuration[Definitions.RABBITMQ_CHANNEL_QOS_PREFETCHSIZE], out var qosPrefetchSize) ?
                qosPrefetchSize : 0;
            var channelQosPrefetchCount = ushort.TryParse(_configuration[Definitions.RABBITMQ_CHANNEL_QOS_PREFETCHCOUNT], out var qosPrefetchCount) ?
                qosPrefetchCount : (ushort)1;
            var channelQosGlobal = bool.TryParse(_configuration[Definitions.RABBITMQ_CHANNEL_QOS_GLOBAL], out var qosGlobal) && qosGlobal;

            channel.ExchangeDeclare(exchangeName, exchangeType);

            channel.QueueDeclare(queue: queueName,
                                 durable: durableValue,
                                 exclusive: exclusiveValue,
                                 autoDelete: autoDeleteValue,
                                 arguments: LoadArguments(Definitions.RABBITMQ_QUEUE_ARGUMENTS));

            channel.BasicQos(prefetchSize: channelQosPrefetchSize, prefetchCount: channelQosPrefetchCount, global: channelQosGlobal);

            channel.QueueBind(queueName, exchangeName, routingKey, LoadArguments(Definitions.RABBITMQ_QUEUE_BIND_ARGUMENTS));

            var consumer = new EventingBasicConsumer(channel);

            consumer.Received += Consumer_Received;

            return consumer;
        }

        /// <summary>
        ///     Load arguments from the configuration file.
        /// </summary>
        /// <param name="sectionName">Section name</param>
        /// <returns>Dictionary of arguments</returns>
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
