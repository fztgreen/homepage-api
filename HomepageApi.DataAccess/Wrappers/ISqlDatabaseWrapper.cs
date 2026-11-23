using Dapper;

namespace HomepageApi.DataAccess.Wrappers;

public interface ISqlDatabaseWrapper
{
    T ExecuteProcedure<T>(string procedureName, DynamicParameters parameters);
}
