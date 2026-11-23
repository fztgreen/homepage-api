using Dapper;

using FluentAssertions;

using HomepageApi.Api.Controllers;
using HomepageApi.Api.Models.Feedback;
using HomepageApi.DataAccess.Feedback;
using HomepageApi.DataAccess.Wrappers;
using HomepageApi.Domain.Commands.Feedback;
using HomepageApi.Domain.DataAccess.Feedback;

using Microsoft.AspNetCore.Mvc;

using NSubstitute;

using RandomTestValues;

namespace HomepageApi.Tests.Api.FeedbackControllerTests;

[TestClass]
public class CreateShould
{
    private FeedbackController _sut;
    private ISqlDatabaseWrapper _sqlDatabase;

    [TestInitialize]
    public void TestInitialize()
    {
        _sqlDatabase = Substitute.For<ISqlDatabaseWrapper>();
        IFeedbackRepository feedbackRepository = new FeedbackRepository(_sqlDatabase);
        IStoreFeedback storeFeedback = new StoreFeedback(feedbackRepository);
        _sut = new FeedbackController(storeFeedback);
    }

    [TestMethod]
    public void ReturnOk()
    {
        // Arrange
        var request = new FeedbackRequest()
        {
            Name = "John Doe",
            Message = "Hello",
            RespondBy = DateTime.Now.AddDays(3),
        };

        // Act
        var result = _sut.Create(request);

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    [TestMethod]
    public void ReturnOk_AndStoreData()
    {
        // Arrange
        var request = new FeedbackRequest()
        {
            Name = RandomValue.String(),
            Message = RandomValue.String(),
            RespondBy = DateTime.Now.AddDays(RandomValue.Int(10, 1))
        };

        // Act
        var result = _sut.Create(request);

        // Assert
        result.Should().BeOfType<OkResult>();
        _sqlDatabase.Received(1).ExecuteProcedure<bool>(Arg.Is("StoreRequest"), Arg.Any<DynamicParameters>());
    }
}
