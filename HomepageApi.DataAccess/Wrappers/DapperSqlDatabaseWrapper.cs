using Dapper;

namespace HomepageApi.DataAccess.Wrappers;

public class DapperSqlDatabaseWrapper : ISqlDatabaseWrapper
{
    public T ExecuteProcedure<T>(string procedureName, DynamicParameters parameters)
    {
        throw new NotImplementedException();
    }
}
