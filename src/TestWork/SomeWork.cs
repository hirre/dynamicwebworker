using WebWorker.Models;
using WebWorkerInterfaces;

namespace Test.TestWork
{
    public class SomeWork : IWork
    {
        public async Task ExecuteWork(IMessage message, CancellationToken stoppingToken)
        {
            // Do some important work here
            Console.WriteLine("Super important work is being done here...");
            await Task.Delay(5000, stoppingToken);
            Console.WriteLine("Super important work done.");
        }
    }
}
