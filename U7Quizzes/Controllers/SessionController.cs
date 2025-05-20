using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;
using U7Quizzes.DTOs.Session;
using U7Quizzes.Extensions;
using U7Quizzes.IServices;


namespace U7Quizzes.Controllers {

    [ApiController]
    [Route("api/[controller]")]
    public class SessionController : ControllerBase
    {

        private readonly ISessionService _seService;

        public SessionController(ISessionService ser) {
            _seService = ser;
            }

        [HttpPost]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public async Task<IActionResult> CreateSession([FromBody] int quizID)
        {
            try
            {
                var userId = User.GetUserId();
                var request = new CreateSessionDTO
                {
                    HostId = userId,
                    QuizId = quizID

                };

                var newSession = await _seService.CreateSession(request);

                return Ok(newSession);

            }

            catch(Exception ex )
            {
                return BadRequest(new { message = ex.Message });
            }
            
        }
        

    }
}

