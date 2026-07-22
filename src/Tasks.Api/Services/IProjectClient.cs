using FluentResults;
using Tasks.Api.Models;

namespace Tasks.Api.Services {
    public interface IProjectClient {
        public Task<Result<ProjectDTO>> GetProjectByIdAsync(Guid projectId);
    }
}
