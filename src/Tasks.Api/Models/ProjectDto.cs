using System.Text.Json.Serialization;

namespace Tasks.Api.Models {
    public class ProjectDto {
        public Guid id {  get; private set; }
        public string name { get; private set; }
        public bool isArchived {  get; private set; }

        [JsonConstructor]
        public ProjectDto(Guid id, string name, bool isArchived) {
            this.id = id;
            this.name = name;
            this.isArchived = isArchived;
        }
    }
}
