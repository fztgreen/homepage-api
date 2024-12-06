using homepageapi.Models;

using HomepageApi.Domain.Commands.Feedback;

using Microsoft.AspNetCore.Mvc;

namespace homepageapi.Controllers;

[ApiController]
[Route("feedback")]
public class FeedbackApi : ControllerBase
{
    private readonly IStoreFeedback _storeFeedback;

    public FeedbackApi(IStoreFeedback storeFeedback)
    {
        _storeFeedback = storeFeedback;
    }

    [HttpPost(Name = "PostFeedback")]
    public IActionResult PostFeedback([FromBody] PostFeedbackRequest request)
    {
        _storeFeedback.WithFeedback();
        return Ok();
    }
}