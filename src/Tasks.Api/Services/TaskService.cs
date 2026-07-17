namespace Tasks.Api.Services {
    public class TaskService : ITaskService {
        private readonly IProjectClient _projectsApiClient;
        public TaskService(IProjectClient projectClient) { 
        this._projectsApiClient = projectClient;
        }
    }
}
