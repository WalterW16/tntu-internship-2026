using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore;
using Projects.Api.Data;
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
        Project result  = _service.CreateProject(project);
        return CreatedAtAction(
         nameof(PostProject),
         new { id = result.id },
         result);
    }

    //GET: api/v1/projects
    [HttpGet]
    [ProducesResponseType(typeof(Project), StatusCodes.Status200OK)]
   public async Task<ActionResult<List<Project>>> GetNonArchivedProjects() {
        List<Project> result = _service.GetListOfNonArchivedProjects();
        return Ok(result);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Project), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Project>> GetProjectById(Guid id) {
        Project project = _service.GetProjectById(id);
        if (project != null) {
            return Ok(project);
        }
        return NotFound(new ProblemDetails{
            Status = StatusCodes.Status404NotFound,
            Title = "Project not found",
            Detail = $"Project with ID '{id}' does not exist."
        });                                 
    }
}
