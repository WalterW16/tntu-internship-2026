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
             result);
        }
        return StatusCode(500, new { Message = "An unexpected error occurred." });
    }

    //GET: api/v1/projects
    [HttpGet]
    [ProducesResponseType(typeof(Project), StatusCodes.Status200OK)]
   public async Task<ActionResult<List<Project>>> GetNonArchivedProjects() {
        var result = await _service.GetListOfNonArchivedProjectsAsync();
        if (result.IsSuccess) {
            return Ok(result);
        }
        return StatusCode(500, new { Message = "An unexpected error occurred." });
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
            return NotFound(new { message = error.Message });
        }
        if (result.IsSuccess) {
            return Ok(result.Value);
        }
        return StatusCode(500, new { Message = "An unexpected error occurred." });
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
            return NotFound(new { message = error.Message });
        }
        if (result.HasError<ConflictError>()) {
            var error = result.Errors.OfType<ConflictError>().First();
            return Conflict(new {message = error.Message});
        }
        if (result.IsSuccess) {
            return Ok(result.Value);
        }
        return StatusCode(500, new { Message = "An unexpected error occurred." });
    }

}
