namespace Tasks.Api.Models {
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
namespace Tasks.Api.Models {
        public class UpdateTaskStatusRequest {
            [Required(ErrorMessage = "Field status is required")]
            [EnumDataType(typeof(TaskItemStatus), ErrorMessage = "Invalid request")]
            [JsonConverter(typeof(JsonStringEnumConverter))]
            public TaskItemStatus Status { get; set; }
        }
     } 
}