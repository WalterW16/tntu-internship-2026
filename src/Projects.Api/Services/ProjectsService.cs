using Projects.Api.Data;
using Projects.Api.Models;

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
    }
}
