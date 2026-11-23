using HomepageApi.Api.Models.Feedback;
using HomepageApi.Domain.Commands.Feedback;

using Microsoft.AspNetCore.Mvc;

namespace HomepageApi.Api.Controllers;

[ApiController]
[Route("feedback")]
public class FeedbackController : ControllerBase
{
    private readonly IStoreFeedback _storeFeedback;

    public FeedbackController(IStoreFeedback storeFeedback)
    {
        _storeFeedback = storeFeedback;
    }

    [HttpPost]
    public IActionResult Create([FromBody] FeedbackRequest request)
    {
        _storeFeedback.WithFeedback();
        return Ok();
    }
}
