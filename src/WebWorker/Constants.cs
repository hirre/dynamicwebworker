namespace WebWorker
{
    public static class Constants
    {
        #region Configuration keys

        public const string WEBWORKER_MAX_WORKERS = "MaxWorkers";
        public const string WEBWORKER_USE_THREADPOOL = "UseThreadPool";
        public const string RABBITMQ_HOSTNAME = "RabbitMQ:HostName";
        public const string RABBITMQ_USERNAME = "RabbitMQ:UserName";
        public const string RABBITMQ_PASSWORD = "RabbitMQ:Password";
        public const string RABBITMQ_PORT = "RabbitMQ:Port";
        public const int RABBITMQ_DEFAULT_PORT = 5672;
        public const string RABBITMQ_AUTORECOVERY = "RabbitMQ:AutomaticRecoveryEnabled";
        public const string RABBITMQ_QUEUE_AUTOACK = "RabbitMQ:Queue:AutoAck";
        public const string RABBITMQ_QUEUE_DURABLE = "RabbitMQ:Queue:Durable";
        public const string RABBITMQ_QUEUE_EXCLUSIVE = "RabbitMQ:Queue:Exclusive";
        public const string RABBITMQ_QUEUE_AUTODELETE = "RabbitMQ:Queue:AutoDelete";
        public const string RABBITMQ_QUEUE_ARGUMENTS = "RabbitMQ:Queue:Arguments";
        public const string RABBITMQ_QUEUE_BIND_ARGUMENTS = "RabbitMQ:Queue:BindArguments";
        public const string RABBITMQ_CHANNEL_EXCHANGETYPE = "RabbitMQ:Channel:ExchangeType";
        public const string RABBITMQ_CHANNEL_QOS_PREFETCHSIZE = "RabbitMQ:Channel:Qos:PrefetchSize";
        public const string RABBITMQ_CHANNEL_QOS_PREFETCHCOUNT = "RabbitMQ:Channel:Qos:PrefetchCount";
        public const string RABBITMQ_CHANNEL_QOS_GLOBAL = "RabbitMQ:Channel:Qos:Global";

        #endregion
    }
}
