using Microsoft.AspNetCore.Mvc;
using WebWorker.Assembly;
using WebWorker.Models;
using WebWorker.Worker;

namespace WebWorker.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WorkerController : ControllerBase
    {
        private IServiceProvider _serviceProvider;
        private WorkerRepo _workerRepo;

        public WorkerController(IServiceProvider serviceProvider,
            IHostApplicationLifetime hostApplicationLifetime,
            WorkerRepo workerRepo)
        {
            _serviceProvider = serviceProvider;
            _workerRepo = workerRepo;
        }

        [HttpPost]
        public IActionResult Post([FromBody] CreateWorkerRequestDto createWorkerRequestDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (_workerRepo.ContainsWorker(createWorkerRequestDto.WorkerId))
                return BadRequest($"Worker {createWorkerRequestDto.WorkerId} already exists.");

            var workerService = new AssemblyWorker(createWorkerRequestDto.WorkerId, _serviceProvider.GetRequiredService<ILogger<AssemblyWorker>>(),
                        _serviceProvider.GetRequiredService<WebWorkerAssemblyLoadContext>());

            var wi = new WorkerInfo(workerService, new CancellationTokenSource());

            _workerRepo.AddWorker(wi);

            _ = workerService.StartAsync(CancellationToken.None);

            return Ok();
        }

        [HttpDelete("{id:guid}")]
        public IActionResult Delete(Guid id)
        {
            if (!_workerRepo.ContainsWorker(id))
                return NotFound($"Worker {id} not found.");

            var workerInfo = _workerRepo.GetWorker(id);

            if (workerInfo == null)
                return BadRequest($"Null worker {id}.");

            workerInfo.AssemblyWorker.StopAsync(workerInfo.CancellationTokenSource.Token);

            return Ok();
        }
    }
}
