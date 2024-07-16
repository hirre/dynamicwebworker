namespace WebWorker.Models
{
    public class CreateWorkerRequestDto
    {
        public Guid WorkerId { get; set; }
        public required string WorkerIdPrefix { get; set; }
    }
}
