using Dapper;

namespace HomepageApi.DataAccess.Wrappers;
public interface IDapperWrapper
{
    T ExecuteProcedure<T>(string procedureName, DynamicParameters parameters);
}
