using HomepageApi.DataAccess.Feedback;
using HomepageApi.DataAccess.Wrappers;
using HomepageApi.Domain.Commands.Feedback;
using HomepageApi.Domain.DataAccess.Feedback;

namespace HomepageApi.IoC;

public static class RegisterDependencies
{
    public static void Register(IServiceCollection services)
    {
        services.AddScoped<IStoreFeedback, StoreFeedback>();
        services.AddScoped<IFeedbackRepository, FeedbackRepository>();
        services.AddScoped<ISqlDatabaseWrapper, DapperSqlDatabaseWrapper>();
    }
}
