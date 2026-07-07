namespace Projects.Api.Models {
    public class Project {
        public Guid id { get; set; }
        public string? name { get; set; }
        public string? description { get; set; }
        public bool isArchived { get; set; }
        public DateTimeOffset? createdAt { get; set; }
    }
}
