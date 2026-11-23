using HomepageApi.DataAccess.Wrappers;
using HomepageApi.Domain.DataAccess.Feedback;
using HomepageApi.Domain.Entities.Feedback;

namespace HomepageApi.DataAccess.Feedback;

public class FeedbackRepository : IFeedbackRepository
{
    private const string StoreFeedbackProcedure = "StoreRequest";

    private readonly ISqlDatabaseWrapper _sqlDatabase;

    public FeedbackRepository(ISqlDatabaseWrapper sqlDatabase)
    {
        _sqlDatabase = sqlDatabase;
    }

    public Task StoreFeedbackAsync(FeedbackSubmission submission)
    {
        var parameters = new StoreFeedbackParameters(submission.Name, submission.Message, submission.RespondBy);
        return _sqlDatabase.ExecuteProcedureAsync(StoreFeedbackProcedure, parameters);
    }
}
