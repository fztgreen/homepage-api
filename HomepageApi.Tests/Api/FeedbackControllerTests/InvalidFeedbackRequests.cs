using HomepageApi.Api.Models.Feedback;

namespace HomepageApi.Tests.Api.FeedbackControllerTests;

public static class InvalidFeedbackRequests
{
    public static IEnumerable<(string InvalidField, Func<FeedbackRequest, FeedbackRequest> Invalidate)> All =>
    [
        ("Name empty", request => request with { Name = string.Empty }),
        ("Name missing", request => request with { Name = null! }),
        ("Message empty", request => request with { Message = string.Empty }),
        ("Message missing", request => request with { Message = null! })
    ];
}
