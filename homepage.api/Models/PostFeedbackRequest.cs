﻿using System.Text.Json.Serialization;

namespace homepage_api.Models;

public class PostFeedbackRequest
{
    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [JsonPropertyName("message")]
    public required string Message { get; set; }

    [JsonPropertyName("respondBy")]
    public DateTime RespondBy { get; set; }
}