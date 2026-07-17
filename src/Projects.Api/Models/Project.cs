using System.Text.Json.Serialization;
namespace Projects.Api.Models {
    public class Project {
        public Guid id { get; private set; }
        public string? name { get; set; }
        public string? description { get; set; }
        public bool isArchived { get; set; }
        public DateTimeOffset? createdAt { get; private set; }

        public Project(string? name, string? description) {
            id = Guid.NewGuid();
            this.name = name;
            this.description = description;
            isArchived = false;
            createdAt = DateTimeOffset.UtcNow;
        }
        [JsonConstructor]
        public Project(Guid id, string? name, string? description, bool isArchived, DateTimeOffset? createdAt) {
            this.id = id;
            this.name = name;
            this.description = description;
            this.isArchived = isArchived;
            this.createdAt = createdAt;
        }
    }
}
