using Microsoft.EntityFrameworkCore.Query;

namespace Tasks.Api.Models {
    public class TaskItem {
        public Guid id { get; private set; }
        public Guid projectId { get; private set; }
        public string title { get; set; }
        public string? description { get; set; }
        public TaskItemStatus status { get; private set; }
        public string? assignee { get; set; }
        public DateTimeOffset? dueDate { get; set; }
        public DateTimeOffset createdAt { get; private set; }
        public DateTimeOffset updatedAt { get; set; }
        public TaskItem(Guid projectId, string title, string description, string? assignee, DateTimeOffset? dueDate) {
            this.id = Guid.NewGuid();
            this.projectId = projectId;
            this.title = title;
            this.description = description;
            this.status = TaskItemStatus.ToDo;
            this.assignee = assignee;
            this.dueDate = dueDate;
            createdAt = DateTimeOffset.UtcNow;
            updatedAt = DateTimeOffset.UtcNow;
        }
        private bool canBeChangedTo(TaskItemStatus status) {
            if (this.status == TaskItemStatus.ToDo && status == TaskItemStatus.InProgress) {
                return true;
            }
            if (this.status == TaskItemStatus.InProgress && status == TaskItemStatus.Done) {
                return true;
            }
            return false;
        }
        public bool setStatus(TaskItemStatus status) {
            if (canBeChangedTo(status)) {
                this.status = status;
                return true;
            }
            return false;
        }
    }
}