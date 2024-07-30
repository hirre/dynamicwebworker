using Microsoft.AspNetCore.Mvc;
using WebWorker.Assembly;
using WebWorker.Models;
using WebWorker.Services.Worker;

namespace WebWorker.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WorkersController(WorkerService workerService, WorkerRepo workerRepo, WorkPluginRepo workerPluginRepo,
        ILogger<WorkersController> logger) : ControllerBase
    {
        private readonly WorkerService _workerService = workerService;
        private readonly WorkerRepo _workerRepo = workerRepo;
        private readonly WorkPluginRepo _workPluginRepo = workerPluginRepo;
        private readonly ILogger<WorkersController> _logger = logger;

        /// <summary>
        ///     Get worker count.
        /// </summary>
        /// <returns>Nr of workers</returns>
        [HttpGet("WorkerCount")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetWorkerCount()
        {
            var nrOfWorkers = _workerRepo.GetWorkerJobCount();

            return Ok(nrOfWorkers);
        }

        /// <summary>
        ///     Get channel count.
        /// </summary>
        /// <returns>Nr of channels</returns>
        [HttpGet("ChannelCount")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetChannelCount()
        {
            var nrOfChannels = _workerRepo.GetChannelCount();

            return Ok(nrOfChannels);
        }

        /// <summary>
        ///     Get work plugin count.
        /// </summary>
        /// <returns>Nr of channels</returns>
        [HttpGet("WorkPluginCount")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetWorkPluginCount()
        {
            var nrOfWorkPlugins = _workPluginRepo.GetWorkPluginCount();

            return Ok(nrOfWorkPlugins);
        }

        /// <summary>
        ///     Get work plugin names.
        /// </summary>
        /// <returns>Nr of channels</returns>
        [HttpGet("WorkPluginNames")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetWorkPluginNames()
        {
            var workPluginNames = _workPluginRepo.GetWorkPluginNames();

            return Ok(workPluginNames);
        }

        /// <summary>
        ///     Creates a worker.
        /// </summary>
        /// <param name="createWorkerRequestDto">Worker request object</param>
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

            var result = await _workerService.CreateWorker(createWorkerRequestDto);

            if (!result.IsSuccess)
            {
                switch (result.ErrorCode)
                {
                    case ErrorCodes.WorkerJobAlreadyExists:
                        return Problem(result.ErrorMessageDetail, statusCode: StatusCodes.Status409Conflict);
                    case ErrorCodes.MaximumWorkersReached:
                        return Problem(result.ErrorMessageDetail, statusCode: StatusCodes.Status429TooManyRequests);
                    default:
                        return Problem(result.ErrorMessageDetail, statusCode: StatusCodes.Status400BadRequest);
                }
            }

            return Ok();
        }

        /// <summary>
        ///     Deletes a worker.
        /// </summary>
        /// <param name="id">Worker id</param>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(string id)
        {
            var result = await _workerService.RemoveWorker(id);

            if (!result.IsSuccess)
            {
                switch (result.ErrorCode)
                {
                    case ErrorCodes.WorkerJobNotFound:
                        return Problem(result.ErrorMessageDetail, statusCode: StatusCodes.Status404NotFound);
                    default:
                        return Problem(result.ErrorMessageDetail, statusCode: StatusCodes.Status400BadRequest);
                }
            }

            return Ok();
        }
    }
}
