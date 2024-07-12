using Microsoft.Extensions.Hosting;

namespace TestWork
{
    public class SomeWork : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Do some important work here
            Console.WriteLine("Super important work is being done here...");
            await Task.Delay(5000, stoppingToken);
            Console.WriteLine("Super important work done.");
        }
    }
}
