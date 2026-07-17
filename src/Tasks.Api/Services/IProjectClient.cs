using Tasks.Api.Models;

namespace Tasks.Api.Services {
    public interface IProjectClient {
        public Task<ProjectDto> GetProjectByIdAsync(Guid projectId, CancellationToken cancellationToken);
    }
}
