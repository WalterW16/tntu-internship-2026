using Tasks.Api.Models;
namespace Tasks.Api.Services {
    public class ProjectClient : IProjectClient{
        private readonly HttpClient _httpClient;
        public ProjectClient(HttpClient httpClient) {
            _httpClient = httpClient;
        }
        public async Task<ProjectDto> GetProjectByIdAsync(Guid projectId, CancellationToken cancellationToken) {
            var response = await _httpClient.GetAsync($"/api/projects/{projectId}", cancellationToken);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound) {
                return null; 
            }
            response.EnsureSuccessStatusCode(); 
            return await response.Content.ReadFromJsonAsync<ProjectDto>(cancellationToken: cancellationToken);
        }
    }
}
