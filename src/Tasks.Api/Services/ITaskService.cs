using FluentResults;
using Tasks.Api.Models;

namespace Tasks.Api.Services {
    public interface ITaskService {
        public Task<Result<TaskItem>> CreateTaskInProjectAsync(Guid projectId, TaskItemRequestDTO requestDTO);
        public Task<Result<List<TaskItem>>> GetListOfTasksForProjectAsync(Guid projectId);
        public Task<Result<TaskItem>> GetTaskByIdInProjectAsync(Guid projectId, Guid taskId);
        public Task<Result<TaskItem>> UpdateTaskDetails(Guid projectId, Guid taskId, TaskItemRequestDTO dro);
        public Task<Result<TaskItem>> ChangeTaskItemStatus(Guid projectId, Guid taskId, TaskItemStatus status);

    }
}