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
        public Project CreateProject(ProjectRequestDTO projectRequestDTO) {
            Project project = new Project(projectRequestDTO.name, projectRequestDTO.description);     
            
            _context.Add(project);
            _context.SaveChanges();
            return project;
        }
        public List<Project> GetListOfNonArchivedProjects() {
            List<Project> list = _context.Projects.Where(p => !p.isArchived).OrderByDescending(p => p.createdAt).ToList();
            return list;
        }
        public Project GetProjectById(Guid guid) {
            Project project = _context.Projects.Find(guid);
            return project;
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

    }
}
