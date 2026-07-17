using Asp.Versioning;
using FluentResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Serialization.HybridRow;
using Microsoft.EntityFrameworkCore;
using Projects.Api.Data;
using Projects.Api.Errors;
using Projects.Api.Models;
using Projects.Api.Services;
using System.Net.NetworkInformation;
using static System.Runtime.InteropServices.JavaScript.JSType;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]

public class ProjectsController : ControllerBase
{
    private readonly IProjectService _service;
    public ProjectsController(IProjectService service)
    {
        _service = service;
    }
    // POST: api/v1/projects
    [HttpPost]
    [ProducesResponseType(typeof(Project), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Project>> PostProject(ProjectRequestDTO project)
    {
        var result  = await _service.CreateProjectAsync(project);
        if (result.IsSuccess) {
            return CreatedAtAction(
             nameof(PostProject),
             new { id = result.Value.id },
             result.Value);
        }
        return Problem(
    detail: "An unexpected error occurred.",
    statusCode: StatusCodes.Status500InternalServerError,
    title: "Internal Server Error"
);
    }

    //GET: api/v1/projects
    [HttpGet]
    [ProducesResponseType(typeof(List<Project>), StatusCodes.Status200OK)]
   public async Task<ActionResult<List<Project>>> GetNonArchivedProjects() {
        var result = await _service.GetListOfNonArchivedProjectsAsync();
        if (result.IsSuccess) {
            return Ok(result.Value);
        }
        return Problem(
    detail: "An unexpected error occurred.",
    statusCode: StatusCodes.Status500InternalServerError,
    title: "Internal Server Error"
);
    }

    //GET: api/v1/projects{id}
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Project), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Project>> GetProjectById(Guid id) {
        var result = await _service.GetProjectByIdAsync(id);
        if (result.HasError<NotFoundError>()) {
            var error = result.Errors.OfType<NotFoundError>().First();
            var problem = new ProblemDetails {
                Status = StatusCodes.Status404NotFound,
                Title = "Project not found", 
                Detail = error.Message       
            };
            return NotFound(problem);
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

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(Project), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<Project>> PutProject(Guid id, ProjectRequestDTO requestDTO) {
     
        var result = await _service.UpdateProjectAsync(id, requestDTO);
        if (result.HasError<NotFoundError>()) {
            var error = result.Errors.OfType<NotFoundError>().First();
            var problem = new ProblemDetails {
                Status = StatusCodes.Status404NotFound,
                Title = "Project not found",
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
        if (result.IsSuccess) {
            return Ok(result.Value);
        }
        return Problem(
    detail: "An unexpected error occurred.",
    statusCode: StatusCodes.Status500InternalServerError,
    title: "Internal Server Error"
);
    }
    [HttpPatch("{id}/archive")]
    [ProducesResponseType(typeof(Project), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<Project>> PatchProject(Guid id) {
        var result = await _service.ArchiveProjectAsync(id);
        if (result.HasError<NotFoundError>()) {
            var error = result.Errors.OfType<NotFoundError>().First();
            var problem = new ProblemDetails {
                Status = StatusCodes.Status404NotFound,
                Title = "Project not found",
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
        if (result.IsSuccess) {
            return Ok(result.Value);
        }
        return Problem(
    detail: "An unexpected error occurred.",
    statusCode: StatusCodes.Status500InternalServerError,
    title: "Internal Server Error"
);
    }
}
