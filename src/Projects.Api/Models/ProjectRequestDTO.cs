using System.ComponentModel.DataAnnotations;
namespace Projects.Api.Models {
    public class ProjectRequestDTO {
        [Required]
        [StringLength(100, MinimumLength = 1)]
        public string? name { get; set; }

        [StringLength(500, MinimumLength = 0)]
        public string? description { get; set; }
    }
}
