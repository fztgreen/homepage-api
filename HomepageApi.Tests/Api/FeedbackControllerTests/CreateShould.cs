using System.Net;
using System.Net.Http.Json;

using FluentAssertions;

using HomepageApi.Api.Models.Feedback;

using NSubstitute;

namespace HomepageApi.Tests.Api.FeedbackControllerTests;

[TestClass]
public class CreateShould : FeedbackControllerTestBase
{
    [TestMethod]
    public async Task ReturnOk_AndStoreFeedback()
    {
        // Arrange
        var request = RandomFeedbackRequest();

        // Act
        var response = await Client.PostAsJsonAsync(FeedbackUri, request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        await SqlDatabase.Received(1).ExecuteProcedureAsync(StoreFeedbackProcedure, ExpectedParameters(request));
    }

    [TestMethod]
    [DynamicData(nameof(InvalidFeedbackRequests.All), typeof(InvalidFeedbackRequests))]
    public async Task ReturnBadRequest_WhenRequestIsInvalid(string invalidField, Func<FeedbackRequest, FeedbackRequest> invalidate)
    {
        // Arrange
        var request = invalidate(RandomFeedbackRequest());

        // Act
        var response = await Client.PostAsJsonAsync(FeedbackUri, request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest, invalidField);
        await SqlDatabase.DidNotReceiveWithAnyArgs().ExecuteProcedureAsync(default!, default!);
    }

    [TestMethod]
    public async Task ReturnInternalServerError_WhenDatabaseFails()
    {
        // Arrange
        var request = RandomFeedbackRequest();
        SqlDatabase.ExecuteProcedureAsync(StoreFeedbackProcedure, ExpectedParameters(request)).Returns(Task.FromException(new InvalidOperationException()));

        // Act
        var response = await Client.PostAsJsonAsync(FeedbackUri, request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }
}
