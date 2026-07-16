using Projects.Api.Models;
using FluentResults;

namespace Projects.Api.Services {
    public interface IProjectService {
        public Task<Result<Project>> CreateProjectAsync(ProjectRequestDTO projectRequestDTO);
        public Task<Result<List<Project>>> GetListOfNonArchivedProjectsAsync();
        public Task<Result<Project>> GetProjectByIdAsync(Guid id); 
        public Task<Result<Project>> UpdateProjectAsync(Guid id, ProjectRequestDTO projectRequestDTO);
        public Task<Result<Project>> ArchiveProjectAsync(Guid id);
    }
}
