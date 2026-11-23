namespace HomepageApi.Domain.DataAccess.Feedback;

public interface IFeedbackRepository
{
    void Store<T>();
}
