using HomepageApi.DataAccess.Wrappers;
using HomepageApi.Domain.DataAccess;

namespace HomepageApi.DataAccess.Repositories;
public class SqlFeedbackRepository : IFeedbackRepository
{
    private readonly IDapperWrapper _dapperWrapper;

    public SqlFeedbackRepository(IDapperWrapper dapperWrapper)
    {
        _dapperWrapper = dapperWrapper;
    }

    public void Store<T>()
    {
        _dapperWrapper.ExecuteProcedure<bool>("StoreRequest", new Dapper.DynamicParameters());
    }
}
