using WebWorker.Models;

namespace WebWorkerInterfaces
{
    public class TestMessage : IMessage
    {
        public required string WorkerId { get; set; }

        public required string Message { get; set; }
    }
}
