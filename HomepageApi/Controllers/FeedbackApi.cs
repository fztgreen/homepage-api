using homepageapi.Models;
using Microsoft.AspNetCore.Mvc;

namespace homepageapi.Controllers;

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
