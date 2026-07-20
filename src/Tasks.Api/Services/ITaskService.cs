using FluentResults;
using Tasks.Api.Models;

namespace Tasks.Api.Services {
    public interface ITaskService {
        public Task<Result<TaskItem>> CreateTaskInProjectAsync(Guid projectId, TaskItemRequestDTO requestDTO);
        public Task<Result<List<TaskItem>>> GetListOfTasksForProjectAsync(Guid projectId);
    }
}