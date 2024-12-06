using Dapper;

using FluentAssertions;

using homepageapi.Controllers;
using homepageapi.Models;

using HomepageApi.DataAccess.Repositories;
using HomepageApi.DataAccess.Wrappers;
using HomepageApi.Domain.Commands.Feedback;
using HomepageApi.Domain.DataAccess;

using Microsoft.AspNetCore.Mvc;

using NSubstitute;

using RandomTestValues;

namespace HomepageApi.tests.Behavioral.FeedbackApiTests;

[TestClass]
[TestCategory("behavior")]
public class PostFeedbackShould
{
    private FeedbackApi? _sut;
    private IDapperWrapper? _dapperWrapperMock;

    [TestInitialize]
    public void TestInitialize()
    {
        _dapperWrapperMock = Substitute.For<IDapperWrapper>();
        IFeedbackRepository feedbackRepository = new SqlFeedbackRepository(_dapperWrapperMock);
        IStoreFeedback storeFeedback = new StoreFeedback(feedbackRepository);
        _sut = new FeedbackApi(storeFeedback);
    }

    [TestMethod]
    public void ReturnOk()
    {
        // Arrange
        var request = new PostFeedbackRequest()
        {
            Name = "John Doe",
            Message = "Hello",
            RespondBy = DateTime.Now.AddDays(3),
        };

        // Act
        var result = _sut.PostFeedback(request);

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    [TestMethod]
    public void ReturnOk_AndStoreData()
    {
        // Arrange
        var request = new PostFeedbackRequest()
        {
            Name = RandomValue.String(),
            Message = RandomValue.String(),
            RespondBy = DateTime.Now.AddDays(RandomValue.Int(10, 1))
        };

        // Act
        var result = _sut.PostFeedback(request);

        // Assert
        result.Should().BeOfType<OkResult>();
        _dapperWrapperMock.Received(1).ExecuteProcedure<bool>(Arg.Is("StoreRequest"), Arg.Any<DynamicParameters>());
    }
}