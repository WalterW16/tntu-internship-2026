using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net.NetworkInformation;
using Tasks.Api.Errors;
using Tasks.Api.Models;
using Tasks.Api.Models.Tasks.Api.Models;
using Tasks.Api.Services;

namespace Tasks.Api.Controllers {

    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/projects")]
    [ApiController]
    public class TasksController : ControllerBase {
        private readonly ITaskService _service;
        public TasksController(ITaskService taskService) {
            _service = taskService;
        }
        [HttpPost("{projectId}/tasks")]
        [ProducesResponseType(typeof(TaskItem), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status502BadGateway)]
        public async Task<ActionResult<TaskItem>> PostTask(Guid projectId, TaskItemRequestDTO requestDTO) {
            var result = await _service.CreateTaskInProjectAsync(projectId, requestDTO);
            if (result.HasError<NotFoundError>()) {
                var error = result.Errors.OfType<NotFoundError>().First();
                var problem = new ProblemDetails {
                    Status = StatusCodes.Status404NotFound,
                    Title = "Resource not found",
                    Detail = error.Message
                };
                return NotFound(problem);
            }
            if (result.HasError<ConflictError>()) {
                var error = result.Errors.OfType<ConflictError>().First();
                var problem = new ProblemDetails {
                    Status = StatusCodes.Status409Conflict,
                    Title = "Conflict",
                    Detail = error.Message
                };
                return Conflict(problem);
            }
            if (result.HasError<BadGatewayError>()) {
                var error = result.Errors.OfType<BadGatewayError>().First();
                var problem = new ProblemDetails {
                    Status = StatusCodes.Status502BadGateway,
                    Title = "BadGateway",
                    Detail = error.Message
                };
                return StatusCode(StatusCodes.Status502BadGateway, problem);
            }
            if (result.IsSuccess) {
                return CreatedAtAction(
                 nameof(PostTask),
                 new { projectId = projectId, id = result.Value.id },
                 result.Value);
            }
            return Problem(
            detail: "An unexpected error occurred.",
            statusCode: StatusCodes.Status500InternalServerError,
            title: "Internal Server Error");
        }
        [HttpGet("{projectId}/tasks")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(List<TaskItem>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status502BadGateway)]
        public async Task<ActionResult<TaskItem>> GetListOfTasksForProject(Guid projectId) {
            var result = await _service.GetListOfTasksForProjectAsync(projectId);
            if (result.HasError<NotFoundError>()) {
                var error = result.Errors.OfType<NotFoundError>().First();
                var problem = new ProblemDetails {
                    Status = StatusCodes.Status404NotFound,
                    Title = "Resource not found",
                    Detail = error.Message
                };
                return NotFound(problem);
            }
            if (result.HasError<BadGatewayError>()) {
                var error = result.Errors.OfType<BadGatewayError>().First();
                var problem = new ProblemDetails {
                    Status = StatusCodes.Status502BadGateway,
                    Title = "BadGateway",
                    Detail = error.Message
                };
                return StatusCode(StatusCodes.Status502BadGateway, problem);
            }
            if (result.IsSuccess) {
                return Ok(result.Value);
            }
            return Problem(
            detail: "An unexpected error occurred.",
            statusCode: StatusCodes.Status500InternalServerError,
            title: "Internal Server Error"
            );
        }
        ///api/v1/projects/{projectId}/tasks/{taskId
        [HttpGet("{projectId}/tasks/{taskId}")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(TaskItem), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status502BadGateway)]
        public async Task<ActionResult<TaskItem>> GetTaskByIdInProject(Guid projectId, Guid taskId) {
            var result = await _service.GetTaskByIdInProjectAsync(projectId, taskId);
            if (result.HasError<NotFoundError>()) {
                var error = result.Errors.OfType<NotFoundError>().First();
                var problem = new ProblemDetails {
                    Status = StatusCodes.Status404NotFound,
                    Title = "Resource not found",
                    Detail = error.Message
                };
                return NotFound(problem);
            }
            if (result.HasError<BadGatewayError>()) {
                var error = result.Errors.OfType<BadGatewayError>().First();
                var problem = new ProblemDetails {
                    Status = StatusCodes.Status502BadGateway,
                    Title = "BadGateway",
                    Detail = error.Message
                };
                return StatusCode(StatusCodes.Status502BadGateway, problem);
            }
            if (result.IsSuccess) {
                return Ok(result.Value);
            }
            return Problem(
            detail: "An unexpected error occurred.",
            statusCode: StatusCodes.Status500InternalServerError,
            title: "Internal Server Error"
            );
        }
        [HttpPut("{projectId}/tasks/{taskId}")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(TaskItem), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status502BadGateway)]
        public async Task<ActionResult<TaskItem>> PutTask(Guid projectId, Guid taskId, TaskItemRequestDTO dto) {
            var result = await _service.UpdateTaskDetails(projectId, taskId, dto);
            if (result.HasError<NotFoundError>()) {
                var error = result.Errors.OfType<NotFoundError>().First();
                var problem = new ProblemDetails {
                    Status = StatusCodes.Status404NotFound,
                    Title = "Resource not found",
                    Detail = error.Message
                };
                return NotFound(problem);
            }
            if (result.HasError<BadGatewayError>()) {
                var error = result.Errors.OfType<BadGatewayError>().First();
                var problem = new ProblemDetails {
                    Status = StatusCodes.Status502BadGateway,
                    Title = "BadGateway",
                    Detail = error.Message
                };
                return StatusCode(StatusCodes.Status502BadGateway, problem);
            }
            if (result.IsSuccess) {
                return Ok(result.Value);
            }
            return Problem(
            detail: "An unexpected error occurred.",
            statusCode: StatusCodes.Status500InternalServerError,
            title: "Internal Server Error"
            );
        }
        [HttpPatch("{projectId}/tasks/{taskId}/status")]
        [ProducesResponseType(typeof(TaskItem), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status502BadGateway)]
        public async Task<ActionResult<TaskItem>> PatchTask(Guid projectId, Guid taskId, [FromBody] UpdateTaskStatusRequest status) {
            var result = await _service.ChangeTaskItemStatus(projectId, taskId, status.Status);
            if (result.HasError<NotFoundError>()) {
                var error = result.Errors.OfType<NotFoundError>().First();
                var problem = new ProblemDetails {
                    Status = StatusCodes.Status404NotFound,
                    Title = "Resource not found",
                    Detail = error.Message
                };
                return NotFound(problem);
            }
            if (result.HasError<BadGatewayError>()) {
                var error = result.Errors.OfType<BadGatewayError>().First();
                var problem = new ProblemDetails {
                    Status = StatusCodes.Status502BadGateway,
                    Title = "BadGateway",
                    Detail = error.Message
                };
                return StatusCode(StatusCodes.Status502BadGateway, problem);
            }
            if (result.HasError<ConflictError>()) {
                var error = result.Errors.OfType<ConflictError>().First();
                var problem = new ProblemDetails {
                    Status = StatusCodes.Status409Conflict,
                    Title = "Conflict",
                    Detail = error.Message
                };
                return Conflict(problem);
            }
            if (result.IsSuccess) {
                return Ok(result.Value);
            }
            return Problem(
            detail: "An unexpected error occurred.",
            statusCode: StatusCodes.Status500InternalServerError,
            title: "Internal Server Error"
            );
        }
        // /api/v1/projects/{projectId}/tasks/{taskId}
        [HttpDelete("{projectId}/tasks/{taskId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status502BadGateway)]
        public async Task<ActionResult> DeleteTask(Guid projectId, Guid taskId) {
            var result = await _service.DeleteTask(projectId, taskId);
            if (result.HasError<NotFoundError>()) {
                var error = result.Errors.OfType<NotFoundError>().First();
                var problem = new ProblemDetails {
                    Status = StatusCodes.Status404NotFound,
                    Title = "Resource not found",
                    Detail = error.Message
                };
                return NotFound(problem);
            }
            if (result.HasError<BadGatewayError>()) {
                var error = result.Errors.OfType<BadGatewayError>().First();
                var problem = new ProblemDetails {
                    Status = StatusCodes.Status502BadGateway,
                    Title = "BadGateway",
                    Detail = error.Message
                };
                return StatusCode(StatusCodes.Status502BadGateway, problem);
            }
            if (result.IsSuccess) {
                return NoContent();
            }
            return Problem(
            detail: "An unexpected error occurred.",
            statusCode: StatusCodes.Status500InternalServerError,
            title: "Internal Server Error"
            );
        }
       }
    }
