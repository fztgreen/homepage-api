using HomepageApi.Domain.Entities.Feedback;

namespace HomepageApi.Domain.DataAccess.Feedback;

public interface IFeedbackRepository
{
    Task StoreFeedbackAsync(FeedbackSubmission submission);
}
