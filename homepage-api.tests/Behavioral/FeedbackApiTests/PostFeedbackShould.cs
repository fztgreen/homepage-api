using FluentAssertions;
using homepage_api.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace homepage_api.tests.Behavioral.FeedbackApiTests;

[TestClass]
[TestCategory("behavior")]
public class PostFeedbackShould
{
    [TestMethod]
    public void ReturnOk()
    {
        // Arrange
        var sut = new FeedbackApi();
        var request = new Models.PostFeedbackRequest
        {
            Name = "John Doe",
            Message = "Hello",
            RespondBy = DateTime.Now.AddDays(3),
        };

        // Act
        var result = sut.PostFeedback(request);

        // Assert
        result.Should().BeOfType<OkResult>();
    }
}
