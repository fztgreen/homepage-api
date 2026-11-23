using HomepageApi.Api.Models.Feedback;
using HomepageApi.Domain.Commands.Feedback;

using Microsoft.AspNetCore.Http;
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
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Create(FeedbackRequest request)
    {
        await _storeFeedback.WithAsync(request.ToFeedbackSubmission());
        return Ok();
    }
}
