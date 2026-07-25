using Projects.Api.Data;
using Projects.Api.Models;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using Projects.Api.Errors;

namespace Projects.Api.Services  {
    public class ProjectsService : IProjectService {
        private readonly ProjectContext _context;
        private readonly ILogger<ProjectsService> _logger;
        public ProjectsService(ProjectContext context, ILogger<ProjectsService> logger) { 
         _context = context;
         _logger = logger;
        }
        public async Task<Result<Project>> CreateProjectAsync(ProjectRequestDTO projectRequestDTO) { 
        Project project = new Project(projectRequestDTO.name, projectRequestDTO.description);     
            await _context.AddAsync(project);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Project {ProjectId} created with name {ProjectName}", project.id, projectRequestDTO.name);
            return Result.Ok(project);
        }
        public async Task<Result<List<Project>>> GetListOfNonArchivedProjectsAsync() {
            List<Project> list = _context.Projects.Where(p => !p.isArchived).OrderByDescending(p => p.createdAt).ToList();
            return Result.Ok(list);
        }
        public async Task<Result<Project>> GetProjectByIdAsync(Guid id) {
            Project project = await _context.Projects.FirstOrDefaultAsync(p => p.id == id);
            if (project == null) {
                return Result.Fail(new NotFoundError("Project with given id does not exist"));
            }
            return Result.Ok(project);
        }
        public async Task<Result<Project>> UpdateProjectAsync(Guid id, ProjectRequestDTO projectRequestDTO)
        {
            Project project = await _context.Projects.FirstOrDefaultAsync(p => p.id == id);
           
            if (project == null) {
                _logger.LogWarning("Update failed, project {ProjectId} not found", id);
                return Result.Fail(new NotFoundError("Project with given id does not exist"));
            }           
            if (project.isArchived) {
                _logger.LogWarning("Update rejected, project {ProjectId} is archived", id);
                return Result.Fail(new ConflictError("Can't update archived project"));
            }
           project.name = projectRequestDTO.name;
           project.description = projectRequestDTO.description;
           await _context.SaveChangesAsync();
            _logger.LogInformation("Project {ProjectId} updated with name {ProjectName}", id, project.name);
            return Result.Ok(project);           

        }
        public async Task<Result<Project>> ArchiveProjectAsync(Guid id) {
            Project project = await _context.Projects.FirstOrDefaultAsync(p => p.id == id);

            if (project == null) {
                _logger.LogWarning("Archive failed, project {ProjectId} not found", id);
                return Result.Fail(new NotFoundError("Project with given id does not exist"));
            }
            if (project.isArchived) {
                _logger.LogWarning("Archive rejected, project {ProjectId} already archived", id);
                return Result.Fail(new ConflictError("Project already archived"));
            }
            project.isArchived = true;
            await _context.SaveChangesAsync();
            _logger.LogInformation("Project {ProjectId} archived", id);
            return Result.Ok(project);
        }
    }
}
