using Projects.Api.Models;

namespace Projects.Api.Services {
    public interface IProjectService {
        public Project CreateProject(ProjectRequestDTO projectRequestDTO);
        public List<Project> GetListOfNonArchivedProjects();
    }
}
