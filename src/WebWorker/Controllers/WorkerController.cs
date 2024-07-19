using Microsoft.AspNetCore.Mvc;
using WebWorker.Logic;
using WebWorker.Models;

namespace WebWorker.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WorkerController(WorkerLogic workLogic, ILogger<WorkerController> logger) : ControllerBase
    {
        private readonly WorkerLogic _workLogic = workLogic;
        private readonly ILogger<WorkerController> _logger = logger;

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] CreateWorkerRequestDto createWorkerRequestDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                await _workLogic.CreateWorker(createWorkerRequestDto);
            }
            catch (DuplicateWaitObjectException ex)
            {
                _logger.LogError(ex, "Error creating worker");

                var problemDetails = new ProblemDetails
                {
                    Title = "Worker already exists",
                    Detail = ex.Message,
                    Status = StatusCodes.Status302Found
                };

                return StatusCode(StatusCodes.Status302Found, problemDetails);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating worker");

                var problemDetails = new ProblemDetails
                {
                    Title = "Other problem",
                    Detail = ex.Message,
                    Status = StatusCodes.Status500InternalServerError
                };

                return StatusCode(StatusCodes.Status500InternalServerError, problemDetails);
            }

            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                await _workLogic.RemoveWorker(id);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogError(ex, "Error deleting worker");

                var problemDetails = new ProblemDetails
                {
                    Title = "Key not found when deleting worker",
                    Detail = ex.Message,
                    Status = StatusCodes.Status404NotFound
                };

                return NotFound(problemDetails);
            }
            catch (NullReferenceException ex)
            {
                _logger.LogError(ex, "Error deleting worker");

                var problemDetails = new ProblemDetails
                {
                    Title = "Worker info null when deleting worker",
                    Detail = ex.Message,
                    Status = StatusCodes.Status404NotFound
                };

                return NotFound(problemDetails);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting worker");

                var problemDetails = new ProblemDetails
                {
                    Title = "Other problem",
                    Detail = ex.Message,
                    Status = StatusCodes.Status500InternalServerError
                };

                return StatusCode(StatusCodes.Status500InternalServerError, problemDetails);
            }

            return Ok();
        }
    }
}
