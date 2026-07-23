using FluentResults;
using Microsoft.EntityFrameworkCore;
using Tasks.Api.Data;
using Tasks.Api.Errors;
using Tasks.Api.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Tasks.Api.Services {
    public class TaskService : ITaskService {
        private readonly IProjectClient _projectsApiClient;
        private readonly TaskContext _context;
        public TaskService(IProjectClient projectClient, TaskContext context) { 
        _context = context;
        _projectsApiClient = projectClient;
        }
        public async Task<Result<TaskItem>> CreateTaskInProjectAsync(Guid projectId, TaskItemRequestDTO requestDTO) {
            var projectClientRequestResult = await _projectsApiClient.GetProjectByIdAsync(projectId);
            if (projectClientRequestResult.HasError<NotFoundError>()) {
                return projectClientRequestResult.Errors.OfType<NotFoundError>().First();
            }
            if (projectClientRequestResult.HasError<BadGatewayError>()) {
                return projectClientRequestResult.Errors.OfType<BadGatewayError>().First();
            }
            if (projectClientRequestResult.IsFailed) {
                return Result.Fail(projectClientRequestResult.Errors.First());
            }
            ProjectDTO projectDto = projectClientRequestResult.Value;
            if (projectDto.isArchived) {
                return Result.Fail(new ConflictError("Can't create task in archived project"));
            }
            TaskItem createdTask = new TaskItem( projectDto.id, requestDTO.title,  requestDTO.description, requestDTO.assignee, requestDTO.dueDate);
            await _context.AddAsync(createdTask);
            await _context.SaveChangesAsync();
            return Result.Ok(createdTask);
        }

        public async Task<Result<List<TaskItem>>> GetListOfTasksForProjectAsync(Guid projectId) {
            var projectClientRequestResult = await _projectsApiClient.GetProjectByIdAsync(projectId);

            if (projectClientRequestResult.HasError<NotFoundError>()) {
                return projectClientRequestResult.Errors.OfType<NotFoundError>().First();
            }
            if (projectClientRequestResult.HasError<BadGatewayError>()) {
                return projectClientRequestResult.Errors.OfType<BadGatewayError>().First();
            }
            if (projectClientRequestResult.IsFailed) {
                return Result.Fail(projectClientRequestResult.Errors.First());
            }
            List<TaskItem> list = _context.TaskItems.Where(p => p.projectId == projectId).OrderByDescending(p => p.createdAt).ToList();
            return Result.Ok(list);
        }
        public async Task<Result<TaskItem>> GetTaskByIdInProjectAsync(Guid projectId, Guid taskId) {
            var projectClientRequestResult = await _projectsApiClient.GetProjectByIdAsync(projectId);

            if (projectClientRequestResult.HasError<NotFoundError>()) {
                return projectClientRequestResult.Errors.OfType<NotFoundError>().First();
            }
            if (projectClientRequestResult.HasError<BadGatewayError>()) {
                return projectClientRequestResult.Errors.OfType<BadGatewayError>().First();
            }
            if (projectClientRequestResult.IsFailed) {
                return Result.Fail(projectClientRequestResult.Errors.First());
            }
            TaskItem task = await _context.TaskItems.FirstOrDefaultAsync(t => t.projectId == projectId && t.id == taskId);
            if (task == null) {
                return Result.Fail(new NotFoundError("No task with specified id"));
            }
            return Result.Ok(task);
        }
        public async Task<Result<TaskItem>> UpdateTaskDetails(Guid projectId, Guid taskId, TaskItemRequestDTO dro) {
            var projectClientRequestResult = await _projectsApiClient.GetProjectByIdAsync(projectId);

            if (projectClientRequestResult.HasError<NotFoundError>()) {
                return projectClientRequestResult.Errors.OfType<NotFoundError>().First();
            }
            if (projectClientRequestResult.HasError<BadGatewayError>()) {
                return projectClientRequestResult.Errors.OfType<BadGatewayError>().First();
            }
            if (projectClientRequestResult.IsFailed) {
                return Result.Fail(projectClientRequestResult.Errors.First());
            }
            TaskItem task = await _context.TaskItems.FirstOrDefaultAsync(t => t.projectId == projectId && t.id == taskId);
            if (task == null) {
                return Result.Fail(new NotFoundError("No task with specified id"));
            }
            task.title = dro.title;
            task.description = dro.description;
            task.assignee = dro.assignee;
            task.dueDate = dro.dueDate;
            await _context.SaveChangesAsync();
            return Result.Ok(task);
        }
        public async Task<Result<TaskItem>> ChangeTaskItemStatus(Guid projectId, Guid taskId, TaskItemStatus status) {
              var projectClientRequestResult = await _projectsApiClient.GetProjectByIdAsync(projectId);

            if (projectClientRequestResult.HasError<NotFoundError>()) {
                return projectClientRequestResult.Errors.OfType<NotFoundError>().First();
            }
            if (projectClientRequestResult.HasError<BadGatewayError>()) {
                return projectClientRequestResult.Errors.OfType<BadGatewayError>().First();
            }
            if (projectClientRequestResult.IsFailed) {
                return Result.Fail(projectClientRequestResult.Errors.First());
            }           
            TaskItem task = await _context.TaskItems.FirstOrDefaultAsync(t => t.projectId == projectId && t.id == taskId);
            if (task == null) {
                return Result.Fail(new NotFoundError("No task with specified id"));
            }
            bool isChanged = task.setStatus(status);
            if (!isChanged) {
                return Result.Fail(new ConflictError($"Can't change status from '{task.status}' to '{status}'"));
            }
            task.updatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return Result.Ok(task);
        }

    }
}
