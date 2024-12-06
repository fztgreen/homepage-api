using Dapper;

namespace HomepageApi.DataAccess.Wrappers;
public class DapperWrapper : IDapperWrapper
{
    public T ExecuteProcedure<T>(string procedureName, DynamicParameters parameters)
    {
        throw new NotImplementedException();
    }
}
