namespace HomepageApi.DataAccess.Wrappers;

public class DapperSqlDatabaseWrapper : ISqlDatabaseWrapper
{
    public Task ExecuteProcedureAsync(string procedureName, object parameters)
    {
        throw new NotImplementedException();
    }
}
