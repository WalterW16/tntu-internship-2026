using System.Reflection;
using System.ComponentModel.DataAnnotations;

namespace Tasks.Api.Models {
    public class TaskItemRequestDTO {
        [Required]
        [StringLength(200, MinimumLength = 1)]
        public string title {  get; set; }

        [StringLength(500, MinimumLength = 0)]
        public string? description { get; set; }

        [StringLength(200, MinimumLength = 0)]
        public string? assignee {  get; set; }
        public DateTimeOffset? dueDate { get; set; }
    }
}
