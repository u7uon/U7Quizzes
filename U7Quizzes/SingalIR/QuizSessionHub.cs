using System.Security.Claims;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using U7Quizzes.DTOs.Quiz;
using U7Quizzes.DTOs.Session;
using U7Quizzes.IRepository;
using U7Quizzes.IServices;
using U7Quizzes.Models;
using U7Quizzes.Repository;

namespace U7Quizzes.SingalIR
{
    public class QuizSessionHub : Hub
    {
        private readonly ISessionService _seesion;
        private readonly IResponseService _responseService;
        private readonly ISessionRepository _seRepository;
        private readonly IMapper mapper;
        private readonly IParticipantRepository _participantRepository; 
        private SemaphoreSlim sem = new SemaphoreSlim(3, 5); 

        public QuizSessionHub(ISessionService seesion, IResponseService responseService , ISessionRepository sessionRepository , IMapper mapper , IParticipantRepository participantRepository)
        {
            _seesion = seesion;
            _responseService = responseService;
            _seRepository = sessionRepository;
            this.mapper = mapper;
            _participantRepository = participantRepository;
        }

        public async Task SubmitAnswer(string AccessCode , ResponseSendDTO request)
        {
            try
            {
                var response = await _responseService.SendResponse(request);

                await Clients.Group($"host_session_{AccessCode}").SendAsync("ResponseSubmited", response);
                await Clients.Caller.SendAsync("AnswerSubmitted",response); 
            }
            catch (Exception ex) {
               await Clients.Caller.SendAsync("Error", ex.Message); 
            }
           
        }

        
        [Authorize(AuthenticationSchemes = "Bearer")]
        public async Task CreateSession(int sessionId)
        {
            try
            {
                var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                var session = await _seRepository.GetSessionByID(sessionId); 

                if(session == null)
                    await Clients.Caller.SendAsync("SessionNotFound");

                else if (session.HostId != userId)
                    await Clients.Caller.SendAsync("SessionNotFound");

                else
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, $"session_{session.AccessCode}");
                    await Groups.AddToGroupAsync(Context.ConnectionId, $"host_session_{session.AccessCode}");
                    await Clients.Caller.SendAsync("SessionCreated", mapper.Map<SessionDTO>(session));
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
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

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task EndSession(int sessionId , string accessCode)
        {
            var sesion = await _seRepository.GetSessionByID(sessionId); 
            await _seesion.EndSession(sessionId);
            //await Clients.Group($"session_{sessionId}").SendAsync("SessionEnded");
            await Clients.Group($"session_{accessCode}").SendAsync("SessionClosed");
        }


        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task SetSessionStatus(string status, int sessionId)
        {
            switch (status)
            {
                case "Pause":
                    await _seesion.PauseSession(sessionId);
                    break;
                case "Resume":
                    await _seesion.ResumeSession(sessionId);
                    break;
                case "Cancel":
                    await _seesion.EndSession(sessionId);
                    break;
                case "Finish":
                    await _seesion.FinishSession(sessionId);
                    break;
                default:
                    throw new HubException("Invalid session status");
            }
        }



        public async Task JoinSession(int paticipantId , string accesscode)
        {
            try
            {
                var participant = await _participantRepository.GetByConnectionId(Context.ConnectionId); 
                if(participant == null)
                {
                    participant = await _participantRepository.GetById(paticipantId);
                    participant.ConnectionId = Context.ConnectionId;
                    await _participantRepository.SaveChangesAsync();

                    var participants = await _seesion.GetParticipants(accesscode);

                    await Clients.Caller.SendAsync("Participants", participants);
                    await Clients.Group($"session_{accesscode}").SendAsync("NewJoined", new ParticipantDTO { ConnectionId = participant.ConnectionId, ParticipantId = participant.ParticipantId, DisplayName = participant.Nickname, UserID = participant.UserId });
                    await Groups.AddToGroupAsync(Context.ConnectionId, $"session_{accesscode}");
                }


               
            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("Error", ex.Message);
            }

        }


        public async Task LeaveSession(int participantId ,string accesscode)
        {
            var par = await _participantRepository.GetById(participantId);

            await Clients.Group($"session_{accesscode}").SendAsync("ParticipantLeft", participantId);
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"session_{accesscode}");

            await _participantRepository.DeleteAsync(par);

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
                    await Task.Delay(question.TimeLimit + 3000);
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
