using Projects.Api.Data;
using Projects.Api.Models;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using Projects.Api.Errors;

namespace Projects.Api.Services  {
    public class ProjectsService : IProjectService {
        private readonly ProjectContext _context;
        public ProjectsService(ProjectContext context) { 
         _context = context;
        }
        public async Task<Result<Project>> CreateProjectAsync(ProjectRequestDTO projectRequestDTO) { 
        Project project = new Project(projectRequestDTO.name, projectRequestDTO.description);     
            await _context.AddAsync(project);
            await _context.SaveChangesAsync();
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
                return Result.Fail(new NotFoundError("Project with given id does not exist"));
            }           
            if (project.isArchived) {
                return Result.Fail(new ConflictError("Can't update archived project"));
            }
           project.name = projectRequestDTO.name;
           project.description = projectRequestDTO.description;
           await _context.SaveChangesAsync();
           return Result.Ok(project);                  

        }
        public async Task<Result<Project>> ArchiveProjectAsync(Guid id) {
            Project project = await _context.Projects.FirstOrDefaultAsync(p => p.id == id);

            if (project == null) {
                return Result.Fail(new NotFoundError("Project with given id does not exist"));
            }
            if (project.isArchived) {
                return Result.Fail(new ConflictError("Project already archived"));
            }
            project.isArchived = true;
            await _context.SaveChangesAsync();
            return Result.Ok(project);
        }
    }
}
