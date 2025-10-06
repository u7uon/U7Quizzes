
using System.CodeDom.Compiler;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using U7Quizzes.AppData;
using U7Quizzes.DTOs.Quiz;
using U7Quizzes.DTOs.Session;
using U7Quizzes.IRepository;
using U7Quizzes.IServices;
using U7Quizzes.Models;
using U7Quizzes.Extensions;
using U7Quizzes.Caching;
using System.Collections.Generic;

namespace U7Quizzes.Services
{
    public class SessionService : ISessionService
    {
        private readonly ApplicationDBContext _context;
        private readonly ISessionRepository _seRepos;
        private readonly IQuizRepository _qRepos;
        private readonly IQuestionsRepository _quesRepos;
        private readonly IMapper _map;
        private readonly IUserRepository _userRepos;
        private readonly IParticipantRepository _participantRepository;

        private readonly ICachingService _cache;

        public SessionService(ISessionRepository seRepos, IQuizRepository quizRepository, ApplicationDBContext context, IUserRepository userRepos, IMapper map, IQuestionsRepository quesRepos, IParticipantRepository repository, ICachingService caching)
        {
            _context = context;
            _seRepos = seRepos;
            _qRepos = quizRepository;
            _userRepos = userRepos;
            _map = map;
            _quesRepos = quesRepos;
            _participantRepository = repository;
            _cache = caching;
        }

        public async Task<SessionDTO> CreateSession(CreateSessionDTO request)
        {
            var quiz = await _qRepos.GetByIdAsync(request.QuizId);

            if (quiz == null)
                throw new NullReferenceException("Quiz does not exists ");

            var accessCode = await GenerateCode();

            var newSession = new Session
            {
                QuizId = request.QuizId,
                HostId = request.HostId,
                AccessCode = accessCode,
                Status = SessionStatus.Waiting,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };

            await _seRepos.AddAsync(newSession);
            await _qRepos.SaveChangesAsync();


            return new SessionDTO
            {
                SessionID = newSession.SessionId,
                QuizId = newSession.QuizId,
                AccessCode = accessCode,
                Status = newSession.Status,
                HostName = "newSession.Host.DisplayName"
            };
        }

        public async Task EndSession(int sessionId)
        {
            var session = await _seRepos.GetSessionByID(sessionId);

            if (session == null)
                throw new NullReferenceException("Session not found");

            session.Status = session.Status != SessionStatus.Cancelled ? SessionStatus.Finished : throw new InvalidOperationException("Session is already finished");


            session.UpdatedAt = DateTime.UtcNow;

            await _seRepos.UpdateAsync(session);



        }

        public async Task FinishSession(int sessionId)
        {
            var session = await _seRepos.GetSessionByID(sessionId);

            if (session == null)
                throw new NullReferenceException("Session not found");

            if (session.Status != SessionStatus.Cancelled || session.Status != SessionStatus.Finished)
            {
                session.Status = SessionStatus.Finished;
                session.UpdatedAt = DateTime.UtcNow;
                session.EndTime = DateTime.UtcNow;

            }
            await _seRepos.UpdateAsync(session);
            throw new InvalidOperationException("Session is already finished or cancelled");


        }

        public async Task<Leaderboard> GetLeaderBoard(int sessionId)
        {
            var leaderBoard = await _cache.Get<Leaderboard>($"leaderboard_{sessionId}");
            if (leaderBoard != null)
                return leaderBoard;

            var sessionExists = await _context.Session
                .AnyAsync(s => s.SessionId == sessionId);

            if (!sessionExists)
                throw new NullReferenceException("Session not found");

            // Query with projection - only fetch what you need
            var participantsWithScores = await _context.Participant
                .Where(p => p.SessionId == sessionId)
                .Select(p => new Participant_Leaderboard
                {
                    Participant = new ParticipantDTO
                    {
                        ParticipantId = p.ParticipantId,
                        DisplayName = p.Nickname,
                        UserID = p.UserId
                    },
                    CorrectAnsCount = p.Responses.Count(r => r.IsCorrect),
                    WrongAwnserCount = p.Responses.Count(r => !r.IsCorrect),
                    Score = p.Responses.Sum(r => r.Score)
                })
                .ToListAsync();


            var rankedParticipants = participantsWithScores
                .OrderByDescending(p => p.Score)
                .ThenByDescending(p => p.CorrectAnsCount)
                .ToList();

            int currentRank = 1;
            for (int i = 0; i < rankedParticipants.Count; i++)
            {
                if (i > 0 && rankedParticipants[i].Score < rankedParticipants[i - 1].Score)
                {
                    currentRank = i + 1;
                }
                rankedParticipants[i].Rank = currentRank;
            }

            leaderBoard = new Leaderboard
            {
                SessionId = sessionId,
                Participants = rankedParticipants
            };
            await _cache.Set(leaderBoard, $"leaderboard_{sessionId}");

            return leaderBoard;
        }

        public async Task<List<ParticipantDTO>> GetParticipants(string AccessCode)
        {
            var participants = await _cache.Get<List<ParticipantDTO>>($"pars_{AccessCode}");

            if (participants == null)
            {
                participants = await _seRepos.GetParticipants(AccessCode);

                await _cache.Set(participants, "$pars_{AccessCode}");
            }

            return participants;

        }

        public async Task<List<ParticipantDTO>> GetParticipantsBySessionId(int sessionId)
        {
            var session = await _seRepos.GetSessionByID(sessionId);
            if (session == null)
                throw new NullReferenceException("Session not found");

            return await GetParticipants(session.AccessCode); 
        }

        public async Task<ParticipantDTO> JoinSession(ParticipantDTO request, string accessCode)
        {

            var session = await _seRepos.GetSesionByCode(accessCode);

            if (session == null)
                throw new Exception("session not exist");


            if (session.SessionStatus != SessionStatus.Waiting.ToString())
            {
                throw new Exception("Can't join this session");
            }

            var newParticipant = new Participant()
            {
                SessionId = session.SessionID,
                ConnectionId = request.ConnectionId,

            };

            if (request.UserID != null)
            {
                var user = await _userRepos.GetUserProfile(request.UserID);

                if (user == null) throw new Exception("User does not exists");

                newParticipant.Nickname = user.DisplayName;
            }

            else
            {
                newParticipant.Nickname = request.DisplayName;
            }

            newParticipant.UserId = request.UserID;

            await _context.Participant.AddAsync(newParticipant);
            await _context.SaveChangesAsync();



            return new ParticipantDTO
            {
                DisplayName = newParticipant.Nickname,
                UserID = newParticipant.UserId,
                ParticipantId = newParticipant.ParticipantId
            };


        }

        public async Task PauseSession(int sessionId)
        {
            var session = await _seRepos.GetSessionByID(sessionId);
            if (session == null)
                throw new NullReferenceException("Session not found");

            if (session.Status != SessionStatus.Cancelled || session.Status != SessionStatus.Finished)
            {
                session.Status = SessionStatus.Paused;
                session.UpdatedAt = DateTime.UtcNow;
                session.EndTime = DateTime.UtcNow;
            }
            await _seRepos.UpdateAsync(session);
            throw new InvalidOperationException("Session is already finished or cancelled");

        }

        public async Task RemoveParticipant(int sessionId, int participantid, string userId)
        {
            var session = await _seRepos.GetSessionByID(sessionId);
            if (session == null)
                throw new NullReferenceException("Session not found");

            if (session.HostId != userId)
                throw new UnauthorizedAccessException("User is not allowed to access this session");

            var par = await _participantRepository.GetBySessionAndId(sessionId, participantid);
            if (par == null)
                throw new NullReferenceException("Participant not found");

            await _participantRepository.DeleteAsync(par);

            await _participantRepository.SaveChangesAsync();
        }

        public async Task ResumeSession(int sessionId)
        {
            var session = await _seRepos.GetSessionByID(sessionId);
            if (session == null)
                throw new NullReferenceException("Session not found");

            if (session.Status == SessionStatus.Paused)
            {
                session.Status = SessionStatus.Active;
                session.UpdatedAt = DateTime.UtcNow;
                session.EndTime = DateTime.UtcNow;
            }
            await _seRepos.UpdateAsync(session);
            throw new InvalidOperationException("Session is already finished or cancelled");

        }

        public async Task<List<QuestionGetDTO>> StartSession(int sessionId, string UserId)
        {
            var session = await _seRepos.GetSessionByID(sessionId);

            if (session == null)
                throw new NullReferenceException("Session not found");

            if (!session.HostId.Equals(UserId))
                throw new UnauthorizedAccessException("User is not allowed to access this session");


            session.Status = SessionStatus.Active;
            session.StartTime = DateTime.UtcNow;

            await _seRepos.UpdateAsync(session);
            await _seRepos.SaveChangesAsync();


            var questions = await _quesRepos.GetQuestionsByQuizId(session.QuizId);


            return questions;

        }

        private async Task<string> GenerateCode()
        {
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var rd = new Random();
            string code;
            bool isExist = false;


            do
            {
                code = new string(Enumerable.Repeat(chars, 6).Select(s => s[rd.Next(chars.Length)]).ToArray());

                isExist = await _context.Session.AnyAsync(x => x.AccessCode == code && !x.IsDeleted);

            }
            while (isExist);
            return code;

        }

    }
}