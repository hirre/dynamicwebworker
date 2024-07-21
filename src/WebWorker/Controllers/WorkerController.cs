using Microsoft.AspNetCore.Mvc;
using WebWorker.Models;
using WebWorker.Services;

namespace WebWorker.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WorkerController(WorkerService workerService, ILogger<WorkerController> logger) : ControllerBase
    {
        private readonly WorkerService _workerService = workerService;
        private readonly ILogger<WorkerController> _logger = logger;

        /// <summary>
        ///     Creates a worker.
        /// </summary>
        /// <param name="createWorkerRequestDto">Worker request object</param>
        /// <returns>200 on success</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Post([FromBody] CreateWorkerRequestDto createWorkerRequestDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _workerService.CreateWorker(createWorkerRequestDto);

            return Ok();
        }

        /// <summary>
        ///     Deletes a worker.
        /// </summary>
        /// <param name="id">Worker id</param>
        /// <returns>200 on success</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(string id)
        {
            await _workerService.RemoveWorker(id);

            return Ok();
        }
    }
}
