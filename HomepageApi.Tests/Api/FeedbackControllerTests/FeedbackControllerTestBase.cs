using HomepageApi.Api.Models.Feedback;
using HomepageApi.DataAccess.Feedback;
using HomepageApi.DataAccess.Wrappers;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

using NSubstitute;

using RandomTestValues;

namespace HomepageApi.Tests.Api.FeedbackControllerTests;

public abstract class FeedbackControllerTestBase
{
    protected const string FeedbackUri = "feedback";
    protected const string StoreFeedbackProcedure = "StoreRequest";

    private WebApplicationFactory<Program> _factory;

    protected ISqlDatabaseWrapper SqlDatabase { get; private set; }

    protected HttpClient Client { get; private set; }

    [TestInitialize]
    public void StartTestServer()
    {
        SqlDatabase = Substitute.For<ISqlDatabaseWrapper>();
        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder => builder.ConfigureTestServices(ReplaceSqlDatabase));
        Client = _factory.CreateClient();
    }

    [TestCleanup]
    public void StopTestServer()
    {
        Client.Dispose();
        _factory.Dispose();
    }

    protected static FeedbackRequest RandomFeedbackRequest()
    {
        return new FeedbackRequest
        {
            Name = RandomValue.String(20),
            Message = RandomValue.String(200),
            RespondBy = RandomValue.DateTime()
        };
    }

    protected static StoreFeedbackParameters ExpectedParameters(FeedbackRequest request)
    {
        return new StoreFeedbackParameters(request.Name, request.Message, request.RespondBy);
    }

    private void ReplaceSqlDatabase(IServiceCollection services)
    {
        services.AddSingleton(SqlDatabase);
    }
}
