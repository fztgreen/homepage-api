using System.ComponentModel.DataAnnotations;

using HomepageApi.Domain.Entities.Feedback;

namespace HomepageApi.Api.Models.Feedback;

public record FeedbackRequest
{
    [Required]
    public string Name { get; init; } = string.Empty;

    [Required]
    public string Message { get; init; } = string.Empty;

    public DateTime RespondBy { get; init; }

    public FeedbackSubmission ToFeedbackSubmission()
    {
        return new FeedbackSubmission(Name, Message, RespondBy);
    }
}
