using System.Text.Json.Serialization;

namespace HomepageApi.Api.Models.Feedback;

public class FeedbackRequest
{
    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [JsonPropertyName("message")]
    public required string Message { get; set; }

    [JsonPropertyName("respondBy")]
    public DateTime RespondBy { get; set; }
}
