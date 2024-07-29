namespace WebWorker.Models
{
    public interface IMessage
    {
        object Data { get; set; }
        string WorkClassName { get; set; }
    }
}
