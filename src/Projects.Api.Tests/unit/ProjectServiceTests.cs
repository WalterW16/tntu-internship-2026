using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;
using Moq;
using Projects.Api.Data;
using Projects.Api.Errors;
using Projects.Api.Models;
using Projects.Api.Services;
using Xunit;

namespace Projects.Api.Tests.unit {
    public class ProjectServiceTests {
        private readonly Mock<ProjectContext> _projectContextMock;
        private readonly ProjectsService _projectsService;

        public ProjectServiceTests() {
            var dummyOptions = new DbContextOptions<ProjectContext>();
            _projectContextMock = new Mock<ProjectContext>(dummyOptions);
            _projectsService = new ProjectsService(_projectContextMock.Object);
        }


        [Fact]
        public async Task CreateProjectAsync_ValidRequest_CreatesProjectAndReturnsOk() {
            var request = new ProjectRequestDTO { name = "Alpha", description = "First project" };

            _projectContextMock.Setup(c => c.SaveChangesAsync(default))
                               .ReturnsAsync(1);

            var result = await _projectsService.CreateProjectAsync(request);

            Assert.True(result.IsSuccess);
            Assert.Equal(request.name, result.Value.name);
            Assert.Equal(request.description, result.Value.description);
            Assert.False(result.Value.isArchived);
            Assert.NotEqual(Guid.Empty, result.Value.id);

            _projectContextMock.Verify(c => c.AddAsync(It.IsAny<Project>(), default), Times.Once);
            _projectContextMock.Verify(c => c.SaveChangesAsync(default), Times.Once);
        }

       [Fact]
        public async Task GetListOfNonArchivedProjectsAsync_ReturnsOnlyNonArchivedAndSortedByDateDesc() {
            var oldDate = DateTimeOffset.UtcNow.AddDays(-2);
            var newDate = DateTimeOffset.UtcNow;

            var projectsList = new List<Project>
            {
                new Project(Guid.NewGuid(), "Archived", "Desc", true, newDate),
                new Project(Guid.NewGuid(), "Old Active", "Desc", false, oldDate),
                new Project(Guid.NewGuid(), "New Active", "Desc", false, newDate)
            };

            var mockDbSet = projectsList.BuildMockDbSet<Project>();
            _projectContextMock.Setup(c => c.Projects).Returns(mockDbSet.Object);

            var result = await _projectsService.GetListOfNonArchivedProjectsAsync();

            Assert.True(result.IsSuccess);
            var returnedProjects = result.Value;

            Assert.Equal(2, returnedProjects.Count);
            Assert.Equal("New Active", returnedProjects.First().name);
            Assert.Equal("Old Active", returnedProjects.Last().name);
        }

        [Fact]
        public async Task GetListOfNonArchivedProjectsAsync_EmptyDatabase_ReturnsOkWithEmptyList() {
            var emptyList = new List<Project>();

            var mockDbSet = emptyList.BuildMockDbSet<Project>();
            _projectContextMock.Setup(c => c.Projects).Returns(mockDbSet.Object);

            var result = await _projectsService.GetListOfNonArchivedProjectsAsync();

            Assert.True(result.IsSuccess);
            Assert.Empty(result.Value);
        }

        [Fact]
        public async Task GetProjectByIdAsync_ProjectExists_ReturnsOkResult() {
            var projectId = Guid.NewGuid();
            var project = new Project(projectId, "Test", "Test", false, DateTimeOffset.UtcNow);

            var mockDbSet = new List<Project> { project }.BuildMockDbSet<Project>();
            _projectContextMock.Setup(c => c.Projects).Returns(mockDbSet.Object);

            var result = await _projectsService.GetProjectByIdAsync(projectId);

            Assert.True(result.IsSuccess);
            Assert.Equal(projectId, result.Value.id);
        }

        [Fact]
        public async Task GetProjectByIdAsync_ProjectDoesNotExist_ReturnsNotFoundError() {
            var mockDbSet = new List<Project>().BuildMockDbSet<Project>();
            _projectContextMock.Setup(c => c.Projects).Returns(mockDbSet.Object);

            var result = await _projectsService.GetProjectByIdAsync(Guid.NewGuid());

            Assert.True(result.IsFailed);
            Assert.IsType<NotFoundError>(result.Errors.First());
        }

        [Fact]
        public async Task UpdateProjectAsync_ValidProject_UpdatesAndSaves() {
            var projectId = Guid.NewGuid();
            var existingProject = new Project(projectId, "Old Name", "Old Desc", false, DateTimeOffset.UtcNow);

            var mockDbSet = new List<Project> { existingProject }.BuildMockDbSet<Project>();
            _projectContextMock.Setup(c => c.Projects).Returns(mockDbSet.Object);

            var updateRequest = new ProjectRequestDTO { name = "New Name", description = "New Desc" };

            var result = await _projectsService.UpdateProjectAsync(projectId, updateRequest);

            Assert.True(result.IsSuccess);
            Assert.Equal("New Name", result.Value.name);
            Assert.Equal("New Desc", result.Value.description);
            _projectContextMock.Verify(c => c.SaveChangesAsync(default), Times.Once);
        }

        [Fact]
        public async Task UpdateProjectAsync_ProjectDoesNotExist_ReturnsNotFoundError() {
            var mockDbSet = new List<Project>().BuildMockDbSet<Project>();
            _projectContextMock.Setup(c => c.Projects).Returns(mockDbSet.Object);

            var updateRequest = new ProjectRequestDTO { name = "New Name", description = "New Desc" };

            var result = await _projectsService.UpdateProjectAsync(Guid.NewGuid(), updateRequest);

            Assert.True(result.IsFailed);
            Assert.IsType<NotFoundError>(result.Errors.First());
            _projectContextMock.Verify(c => c.SaveChangesAsync(default), Times.Never);
        }

        [Fact]
        public async Task UpdateProjectAsync_ProjectIsArchived_ReturnsConflictError() {
            var projectId = Guid.NewGuid();
            var archivedProject = new Project(projectId, "Archived", "Desc", true, DateTimeOffset.UtcNow);

            var mockDbSet = new List<Project> { archivedProject }.BuildMockDbSet<Project>();
            _projectContextMock.Setup(c => c.Projects).Returns(mockDbSet.Object);

            var updateRequest = new ProjectRequestDTO { name = "New Name", description = "New Desc" };

            var result = await _projectsService.UpdateProjectAsync(projectId, updateRequest);

            Assert.True(result.IsFailed);
            Assert.IsType<ConflictError>(result.Errors.First());
            _projectContextMock.Verify(c => c.SaveChangesAsync(default), Times.Never);
        }

        [Fact]
        public async Task ArchiveProjectAsync_ValidProject_ArchivesAndSaves() {
            var projectId = Guid.NewGuid();
            var activeProject = new Project(projectId, "Active", "Desc", false, DateTimeOffset.UtcNow);

            var mockDbSet = new List<Project> { activeProject }.BuildMockDbSet<Project>();
            _projectContextMock.Setup(c => c.Projects).Returns(mockDbSet.Object);

            var result = await _projectsService.ArchiveProjectAsync(projectId);

            Assert.True(result.IsSuccess);
            Assert.True(result.Value.isArchived);
            _projectContextMock.Verify(c => c.SaveChangesAsync(default), Times.Once);
        }

        [Fact]
        public async Task ArchiveProjectAsync_ProjectAlreadyArchived_ReturnsConflictError() {
            var projectId = Guid.NewGuid();
            var archivedProject = new Project(projectId, "Archived", "Desc", true, DateTimeOffset.UtcNow);

            var mockDbSet = new List<Project> { archivedProject }.BuildMockDbSet<Project>();
            _projectContextMock.Setup(c => c.Projects).Returns(mockDbSet.Object);

            var result = await _projectsService.ArchiveProjectAsync(projectId);

            Assert.True(result.IsFailed);
            Assert.IsType<ConflictError>(result.Errors.First());
            _projectContextMock.Verify(c => c.SaveChangesAsync(default), Times.Never);
        }

        [Fact]
        public async Task ArchiveProjectAsync_ProjectDoesNotExist_ReturnsNotFoundError() {
            var mockDbSet = new List<Project>().BuildMockDbSet<Project>();
            _projectContextMock.Setup(c => c.Projects).Returns(mockDbSet.Object);

            var result = await _projectsService.ArchiveProjectAsync(Guid.NewGuid());

            Assert.True(result.IsFailed);
            Assert.IsType<NotFoundError>(result.Errors.First());
            _projectContextMock.Verify(c => c.SaveChangesAsync(default), Times.Never);
        }
    }
}