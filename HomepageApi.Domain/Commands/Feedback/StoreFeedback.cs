using HomepageApi.Domain.DataAccess.Feedback;
using HomepageApi.Domain.Entities.Feedback;

namespace HomepageApi.Domain.Commands.Feedback;

public class StoreFeedback : IStoreFeedback
{
    private readonly IFeedbackRepository _feedbackRepository;

    public StoreFeedback(IFeedbackRepository feedbackRepository)
    {
        _feedbackRepository = feedbackRepository;
    }

    public Task WithAsync(FeedbackSubmission submission)
    {
        return _feedbackRepository.StoreFeedbackAsync(submission);
    }
}
