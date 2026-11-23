using HomepageApi.DataAccess.Wrappers;
using HomepageApi.Domain.DataAccess.Feedback;

namespace HomepageApi.DataAccess.Feedback;

public class FeedbackRepository : IFeedbackRepository
{
    private readonly ISqlDatabaseWrapper _sqlDatabase;

    public FeedbackRepository(ISqlDatabaseWrapper sqlDatabase)
    {
        _sqlDatabase = sqlDatabase;
    }

    public void Store<T>()
    {
        _sqlDatabase.ExecuteProcedure<bool>("StoreRequest", new Dapper.DynamicParameters());
    }
}
