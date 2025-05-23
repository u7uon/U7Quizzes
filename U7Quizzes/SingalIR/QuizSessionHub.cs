using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using U7Quizzes.DTOs.Session;
using U7Quizzes.IServices;

namespace U7Quizzes.SingalIR
{
    public class QuizSessionHub : Hub
    {
        private readonly ISessionService _seesion;

        public QuizSessionHub(ISessionService seesion)
        {
            _seesion = seesion;
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task CreateSession(int quizId)
        {
            try
            {
                var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    throw new HubException("User not authenticated");

                var newSession = await _seesion.CreateSession(new CreateSessionDTO
                {
                    HostId = userId,
                    QuizId = quizId
                });


                await Groups.AddToGroupAsync(Context.ConnectionId, $"session_{newSession.AccessCode}");
                await Clients.Caller.SendAsync("SessionCreated",newSession );
            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("Error", ex.Message);
            }
        }



        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task StartSession(int sessionId)
        {
            try
            {
                var questions = await _seesion.StartSession(sessionId, GetUserID());

                await Clients.Caller.SendAsync("SessionStarted", questions);
            }

            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("Error", ex.Message);
            }
        }



        [Authorize(AuthenticationSchemes = "Bearer")]
        public async Task JoinSessionWithAuth(string accesscode)
        {
            try
            {
                var UserID = Context.User?.FindFirst(ClaimTypes.NameIdentifier).Value;

                var newParticipant = new ParticipantDTO { UserID = UserID };

                var joined =  await _seesion.JoinSession(newParticipant, accessCode: accesscode);
                var participants = await _seesion.GetParticipants(accesscode);

                await Clients.Caller.SendAsync("Participants", participants);
                await Groups.AddToGroupAsync(Context.ConnectionId, $"session_{accesscode}");
                await Clients.Group($"session_{accesscode}").SendAsync("NewJoined", joined);

            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("Error", ex.Message);
            }


        }
        



        public  async Task JoinSession(string DisplayName, string accesscode)
        {
            try
            {

                var newParticipant = new ParticipantDTO {DisplayName = DisplayName};

                var joined =   await _seesion.JoinSession(newParticipant, accessCode: accesscode);
                
                var participants = await _seesion.GetParticipants(accesscode);

                await Clients.Caller.SendAsync("Participants", participants);
                await Groups.AddToGroupAsync(Context.ConnectionId, $"session_{accesscode}");

                await Clients.Group($"session_{accesscode}").SendAsync("NewJoined", joined);

            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("Error", ex.Message);
            }


        }



        private string GetUserID()
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                throw new HubException("User not authenticated");

            return userId; 
        }
        
        
    }
}
