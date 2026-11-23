using HomepageApi.Domain.Entities.Feedback;

namespace HomepageApi.Domain.Commands.Feedback;

public interface IStoreFeedback
{
    Task WithAsync(FeedbackSubmission submission);
}
