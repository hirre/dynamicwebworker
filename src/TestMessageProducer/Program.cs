using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using WebWorkerInterfaces;

var factory = new ConnectionFactory { HostName = "localhost" };
factory.AutomaticRecoveryEnabled = true;
using var connection = factory.CreateConnection();
using var channel = connection.CreateModel();

// Enable publisher confirms
channel.ConfirmSelect();

var queueName = "rco.sys.66ffb158-b224-48b7-83ab-e5c4b6907ebd";
var exchangeName = "exchange." + queueName;
var routeKey = "route." + queueName;

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

while (true)
{
    var msg = new TestMessage
    {
        Message = "Hello World!"
    };

    var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(msg));

    channel.BasicPublish(exchange: exchangeName,
                         routingKey: routeKey,
                         basicProperties: null,
                         body: body);

    // Wait for the message to be confirmed
    if (channel.WaitForConfirms())
    {
        Console.WriteLine("Message sent and confirmed.");
    }
    else
    {
        Console.WriteLine("Message sent but not confirmed.");
    }

    Console.WriteLine($" [x] Sent {msg.Message}");
    Console.ReadLine();

}