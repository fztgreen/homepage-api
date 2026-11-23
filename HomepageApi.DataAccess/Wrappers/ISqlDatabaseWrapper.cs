namespace HomepageApi.DataAccess.Wrappers;

public interface ISqlDatabaseWrapper
{
    Task ExecuteProcedureAsync(string procedureName, object parameters);
}
