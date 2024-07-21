using Microsoft.AspNetCore.Mvc;
using WebWorker.Models;
using WebWorker.Services.Worker;

namespace WebWorker.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WorkersController(WorkerService workerService, ILogger<WorkersController> logger) : ControllerBase
    {
        private readonly WorkerService _workerService = workerService;
        private readonly ILogger<WorkersController> _logger = logger;

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
