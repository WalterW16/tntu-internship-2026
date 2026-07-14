using Projects.Api.Models;
using FluentResults;

namespace Projects.Api.Services {
    public interface IProjectService {
        public Project CreateProject(ProjectRequestDTO projectRequestDTO);
        public List<Project> GetListOfNonArchivedProjects();
        public Project GetProjectById(Guid guid); 

        public Task<Result<Project>> UpdateProjectAsync(Guid id, ProjectRequestDTO projectRequestDTO);
    }
}
