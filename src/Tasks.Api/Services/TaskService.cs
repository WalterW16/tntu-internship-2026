using FluentResults;
using Microsoft.EntityFrameworkCore;
using Tasks.Api.Data;
using Tasks.Api.Errors;
using Tasks.Api.Models;

namespace Tasks.Api.Services {
    public class TaskService : ITaskService {
        private readonly IProjectClient _projectsApiClient;
        private readonly TaskContext _context;
        private readonly ILogger<TaskService> _logger;
        public TaskService(IProjectClient projectClient, TaskContext context, ILogger<TaskService> logger) { 
        _context = context;
        _projectsApiClient = projectClient;
        _logger = logger;
        }
        public async Task<Result<TaskItem>> CreateTaskInProjectAsync(Guid projectId, TaskItemRequestDTO requestDTO) {
            var projectResult = await GetValidatedProjectAsync(projectId, "Task creation");
            if (projectResult.IsFailed) {
                return Result.Fail(projectResult.Errors);
            }
            ProjectDTO projectDto = projectResult.Value;
            if (projectDto.isArchived) {
                _logger.LogWarning("Task creation rejected, project {ProjectId} is archived", projectId);
                return Result.Fail(new ConflictError("Can't create task in archived project"));
            }
            TaskItem createdTask = new TaskItem( projectDto.id, requestDTO.title,  requestDTO.description, requestDTO.assignee, requestDTO.dueDate);
            await _context.AddAsync(createdTask);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Task {TaskId} created with title {TaskName} in project {ProjectId}", createdTask.id, createdTask.title, createdTask.projectId);
            return Result.Ok(createdTask);
        }

        public async Task<Result<List<TaskItem>>> GetListOfTasksForProjectAsync(Guid projectId) {
            var projectResult = await GetValidatedProjectAsync(projectId, "Task list");
            if (projectResult.IsFailed) {
                return Result.Fail(projectResult.Errors);
            }
            List<TaskItem> list = await _context.TaskItems.Where(p => p.projectId == projectId).OrderByDescending(p => p.createdAt).ToListAsync();
            return Result.Ok(list);
        }
        public async Task<Result<TaskItem>> GetTaskByIdInProjectAsync(Guid projectId, Guid taskId) {
            var projectResult = await GetValidatedProjectAsync(projectId, "Task retrieval");
            if (projectResult.IsFailed) {
                return Result.Fail(projectResult.Errors);
            }
            TaskItem task = await _context.TaskItems.FirstOrDefaultAsync(t => t.projectId == projectId && t.id == taskId);
            if (task == null) {
                _logger.LogWarning("Task retrieval failed, task {TaskId} not found", taskId);
                return Result.Fail(new NotFoundError("No task with specified id"));
            }
            return Result.Ok(task);
        }
        public async Task<Result<TaskItem>> UpdateTaskDetailsAsync(Guid projectId, Guid taskId, TaskItemRequestDTO dro) {
            var projectResult = await GetValidatedProjectAsync(projectId, "Task update");
            if (projectResult.IsFailed) {
                return Result.Fail(projectResult.Errors);
            }
            TaskItem task = await _context.TaskItems.FirstOrDefaultAsync(t => t.projectId == projectId && t.id == taskId);
            if (task == null) {
                _logger.LogWarning("Update failed, task {TaskId} not found", taskId);
                return Result.Fail(new NotFoundError("No task with specified id"));
            }
            task.title = dro.title;
            task.description = dro.description;
            task.assignee = dro.assignee;
            task.dueDate = dro.dueDate;
            task.updatedAt= DateTimeOffset.UtcNow;
            await _context.SaveChangesAsync();
            _logger.LogInformation("Task {TaskId} updated", taskId);
            return Result.Ok(task);
        }
        public async Task<Result<TaskItem>> ChangeTaskItemStatusAsync(Guid projectId, Guid taskId, TaskItemStatus status) {
            var projectResult = await GetValidatedProjectAsync(projectId, "Task status change");
            if (projectResult.IsFailed) {
                return Result.Fail(projectResult.Errors);
            }
            TaskItem task = await _context.TaskItems.FirstOrDefaultAsync(t => t.projectId == projectId && t.id == taskId);          
            if (task == null) {
                _logger.LogWarning("Status change failed, task {TaskId} not found", taskId);
                return Result.Fail(new NotFoundError("No task with specified id"));
            }
            var oldStatus = task.status;
            bool isChanged = task.SetStatus(status);
            if (!isChanged) {
                _logger.LogWarning("Rejected invalid transition for task {TaskId}: {From} -> {To}", taskId, oldStatus, status);
                return Result.Fail(new ConflictError($"Can't change status from '{task.status}' to '{status}'"));
            }
            task.updatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            _logger.LogInformation("Task {TaskId} transitioned {From} -> {To}", taskId, oldStatus, status);
            return Result.Ok(task);
        }
        public async Task<Result> DeleteTaskAsync(Guid projectId, Guid taskId) {
            var projectResult = await GetValidatedProjectAsync(projectId, "Task deletion");
            if (projectResult.IsFailed) {
                return Result.Fail(projectResult.Errors);
            }
            TaskItem task = await _context.TaskItems.FirstOrDefaultAsync(t => t.projectId == projectId && t.id == taskId);
            if (task == null) {
                _logger.LogWarning("Delete failed, task {TaskId} not found", taskId);
                return Result.Fail(new NotFoundError("No task with specified id"));
            }
            _context.TaskItems.Remove(task);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Task {TaskId} deleted", taskId);
            return Result.Ok();
        }
        public async Task<Result<List<TaskItem>>> FilterByStatusAsync(Guid projectId, TaskItemStatus status) {
            var projectResult = await GetValidatedProjectAsync(projectId, "Task filter");
            if (projectResult.IsFailed) {
                return Result.Fail(projectResult.Errors);
            }
            List<TaskItem> filteredList = await _context.TaskItems.Where(p => p.status==status).ToListAsync();
            return Result.Ok(filteredList);
        }
        private async Task<Result<ProjectDTO>> GetValidatedProjectAsync(Guid projectId, string operationName) {
            var projectResult = await _projectsApiClient.GetProjectByIdAsync(projectId);

            if (projectResult.HasError<NotFoundError>()) {
                _logger.LogWarning("{OperationName} failed, project {ProjectId} not found", operationName, projectId);
                return Result.Fail(projectResult.Errors.OfType<NotFoundError>().First());
            }
            if (projectResult.HasError<BadGatewayError>()) {
                return Result.Fail(projectResult.Errors.OfType<BadGatewayError>().First());
            }
            if (projectResult.IsFailed) {
                return Result.Fail(projectResult.Errors);
            }
            return Result.Ok(projectResult.Value);
        }
    }
}
