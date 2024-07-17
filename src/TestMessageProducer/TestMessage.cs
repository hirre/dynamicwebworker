using WebWorker.Models;

namespace TestMessageProducer
{
    public class TestMessage : IMessage
    {
        public required string Message { get; set; }

        public required string Id { get; set; }
    }
}
