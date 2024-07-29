using WebWorker.Models;

namespace WebWorkerInterfaces
{
    public class WebWorkerMessage : IMessage
    {
        public required object Data { get; set; }
        public required string WorkClassName { get; set; }
    }
}
