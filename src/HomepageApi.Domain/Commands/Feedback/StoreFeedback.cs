using HomepageApi.Domain.DataAccess;

namespace HomepageApi.Domain.Commands.Feedback;
public class StoreFeedback : IStoreFeedback
{
    private readonly IFeedbackRepository _feedbackRepository;

    public StoreFeedback(IFeedbackRepository feedbackRepository)
    {
        _feedbackRepository = feedbackRepository;
    }

    public void WithFeedback()
    {
        _feedbackRepository.Store<bool>();
    }
}
