using System.Text.Json.Serialization;

namespace Tasks.Api.Models {
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum TaskItemStatus {
        ToDo = 0,
        InProgress =1,
        Done =2
    }
}
