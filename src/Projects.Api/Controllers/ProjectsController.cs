using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Projects.Api.Data;
using Projects.Api.Models;
using Projects.Api.Services;

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
}
