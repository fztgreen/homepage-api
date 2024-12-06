namespace HomepageApi.Domain.DataAccess;

public interface IRepository
{
    void Store<T>();
}