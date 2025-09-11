using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using U7Quizzes.DTOs.Quiz;
using U7Quizzes.DTOs.Session;
using U7Quizzes.IRepository;
using U7Quizzes.IServices;

namespace U7Quizzes.SingalIR
{
    public class QuizSessionHub : Hub
    {
        private readonly ISessionService _seesion;
        private SemaphoreSlim sem = new SemaphoreSlim(3, 5); 

        public QuizSessionHub(ISessionService seesion)
        {
            _seesion = seesion;
        }

        public async Task SubmitAnswer(string AccessCode , ResponseSendDTO request)
        {
            try
            {
                var response = await _responseService.SendResponse(request);

                await Clients.Group($"host_session_{AccessCode}").SendAsync("ResponseSubmited", response);
            }
            catch (Exception ex) {
               await Clients.Caller.SendAsync("Error", ex.Message); 
            }
           
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
                await Groups.AddToGroupAsync(Context.ConnectionId, $"host_session_{newSession.AccessCode}");
                await Clients.Caller.SendAsync("SessionCreated", newSession);
            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("Error", ex.Message);
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task StartSession(int sessionId ,string accessCode)
        {
            try
            {
                var questions = await _seesion.StartSession(sessionId, GetUserID());

                await Clients.Group($"session_{accessCode}").SendAsync("SessionStarted");

                // Capture the current context
                var groupClients = Clients.Group($"session_{accessCode}");

                // Start the question sequence without Task.Run
                _ = SendQuestionsSequentially(groupClients, questions, accessCode);
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

        public  async Task JoinSession( string accesscode)
        {
            try
            {

                var joined =   await _seesion.JoinSession(newParticipant, accessCode: accesscode);

                await Clients.Caller.SendAsync("Joined", joined);

                await Groups.AddToGroupAsync(Context.ConnectionId, $"session_{accesscode}");

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.InnerException?.Message); 
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

        private async Task SendQuestionsSequentially(IClientProxy groupClients, List<QuestionGetDTO> questions, string accessCode)
        {
            try
            {
                await Task.Delay(3000);
                Console.WriteLine("Starting question sequence");
                Console.WriteLine($"Total questions: {questions.Count}");

                foreach (var question in questions)
                {
                    Console.WriteLine($"Sending question: {question.Content}");
                    await groupClients.SendAsync("ReceiveQuestion", question);

                    // Wait for the time limit of this question
                    await Task.Delay(question.TimeLimit);
                }

                Console.WriteLine("Session ended");
                await groupClients.SendAsync("SessionEnded");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in question sequence: {ex.Message}");
                await groupClients.SendAsync("Error", $"Question sequence error: {ex.Message}");
            }
        }

    }
}
