using WebWorker.Models;

namespace WebWorkerInterfaces
{
    public class TestMessage : IMessage
    {
        public required string Message { get; set; }
        public required string WorkClassName { get; set; }
    }
}
