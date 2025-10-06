using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Identity.Client;
using System.Net.WebSockets;
using U7Quizzes.Attribute;
using U7Quizzes.DTOs.Session;
using U7Quizzes.Extensions;
using U7Quizzes.IRepository;
using U7Quizzes.IServices;
using U7Quizzes.SingalIR;


namespace U7Quizzes.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class SessionController : ControllerBase
    {

        private readonly ISessionService _seService;
        private readonly ISessionRepository _sRepository;
        private readonly IHubContext<QuizSessionHub> hub;

        public SessionController(ISessionService ser, ISessionRepository _sRepository, IHubContext<QuizSessionHub> hub)
        {
            _seService = ser;
            this._sRepository = _sRepository;
            this.hub = hub;
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public async Task<IActionResult> CreateSession([FromBody] CreateSessionRequest create)
        {
            try
            {
                var userId = User.GetUserId();
                var request = new CreateSessionDTO
                {
                    HostId = userId,
                    QuizId = create.QuizId
                };

                var newSession = await _seService.CreateSession(request);

                return Ok(new { sessionId = newSession.SessionID });

            }

            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }

        }


        [HttpGet("{sessionId}/participants")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public async Task<IActionResult> GetParticipants(int sessionId)
        {
            try
            {
                var participants = await _seService.GetParticipantsBySessionId(sessionId);
                return Ok(participants);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("participants")]
        public async Task<IActionResult> GetParticipantsByAcesscode([FromQuery]string acesscode)
        {
            try
            {
                var participants = await _seService.GetParticipants(acesscode);
                return Ok(participants);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }


        [HttpDelete("{sessionId}/participants/{participantId}")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public async Task<IActionResult> RemovePaticipant(int sessionId, int participantId)
        {
            try
            {
                var session = await _sRepository.GetSessionByID(sessionId);

                await _seService.RemoveParticipant(sessionId, participantId, User.GetUserId());

                await hub.Clients.Group($"session_{session.AccessCode}").SendAsync("ParticipantRemoved", participantId);

                return NoContent();


            }
            catch
            {
                return BadRequest(new { message = "An error occurred while removing the participant." });

            }


        }

        [OptionalAuth]
        [HttpPost("join")]
        public async Task<IActionResult> AddParticipant([FromBody] AddParticipantRequest request)
        {
            try
            {
                var participant = await _seService.JoinSession(new ParticipantDTO
                {
                    DisplayName = request.DisplayName,
                    UserID = User.GetUserId(),
                }, request.AccessCode);
                return Ok(new {participantId = participant.ParticipantId});
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }


        //[HttpGet("{id}/leaderboard")]
        //public async Task<IActionResult> GetSessionLeadrboard(int id)
        //{
        //    if(id == 0)
        //        return BadRequest();

        //    var leaderboard = await 


        //}

        public class AddParticipantRequest
        {
            public string AccessCode { get; set; }
            public string DisplayName { get; set; }
        }

        public class CreateSessionRequest
        {
            public int QuizId { get; set; }
        }
    }
}

