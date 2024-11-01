using homepage_api.Models;
using Microsoft.AspNetCore.Mvc;

namespace homepage_api.Controllers;

[ApiController]
[Route("feedback")]
public class FeedbackApi : ControllerBase
{
    [HttpPost(Name = "PostFeedback")]
    public IActionResult PostFeedback([FromBody] PostFeedbackRequest request)
    {
        return Ok();
    }
}
