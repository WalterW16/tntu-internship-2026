using FluentResults;
using System.Net;
using Tasks.Api.Errors;
using Tasks.Api.Models;

namespace Tasks.Api.Services {
    public class ProjectClient : IProjectClient {
        private readonly HttpClient _httpClient;

        public ProjectClient(HttpClient httpClient) {
            _httpClient = httpClient;
        }
        public async Task<Result<ProjectDTO>> GetProjectByIdAsync(Guid projectId) {
            try {
                var response = await _httpClient.GetAsync($"/api/v1/projects/{projectId}");
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound) {
                    return Result.Fail(new NotFoundError("Project with given id does not exist"));
                }
                if (!response.IsSuccessStatusCode) {
                    return Result.Fail($"Projects API returned an error: {response.StatusCode}");
                }
                var data = await response.Content.ReadFromJsonAsync<ProjectDTO>();
                if (data == null) {
                    return Result.Fail("Failed to deserialize Project API response.");
                }
                return Result.Ok(data);
            } catch (HttpRequestException) {
                return Result.Fail(new BadGatewayError("Projects API is unavailable."));
            } catch (Exception ex) {
                return Result.Fail($"Unexpected error: {ex.Message}");
            }
        }
    }
}