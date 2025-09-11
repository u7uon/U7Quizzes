using AutoMapper;
using U7Quizzes.DTOs.Session;
using U7Quizzes.IRepository;
using U7Quizzes.IServices;
using U7Quizzes.Models;
using U7Quizzes.Repository;

namespace U7Quizzes.Services
{
    public class ResponseService : IResponseService
    {
        private readonly IQuestionsRepository _quesRepos;
        private readonly IAnswerRepository _answerRepos;
        private readonly IResponseRepository _responseRepos;

        private readonly IMapper _map;

        public ResponseService(IQuestionsRepository quesRepos, IAnswerRepository answerRepos, IResponseRepository responseRepos, IMapper map)
        {
            _answerRepos = answerRepos;
            _quesRepos = quesRepos;
            _responseRepos = responseRepos;
            _map = map;
        }

        public async Task<ResponseDTO> SendResponse(ResponseSendDTO request)
        {
            if (await _responseRepos.IsResponseSubmitted(request.ParticipantId, request.QuestionID)) {
                throw new Exception("Already submit response");
            }

            Response newResponse;

            if (request.AnswerId.HasValue)
            {
                var answer = await _answerRepos.GetAnswerById(request.AnswerId.Value);
                if (answer == null || answer.Question.QuestionId != request.QuestionID)
                    throw new Exception("Answer or question not valid ");

                newResponse = new Response
                {
                    ParticipantId = request.ParticipantId,
                    QuestionId = answer.Question.QuestionId,
                    AnswerId = request.AnswerId,
                    IsCorrect = answer.IsCorrect,
                    Score = answer.IsCorrect ? answer.Question.Points : 0
                };
            }

            else
            {
                newResponse = new Response
                {
                    ParticipantId = request.ParticipantId,
                    QuestionId = request.QuestionID,
                    IsCorrect = false,
                    Score = 0
                };

            }
            await _responseRepos.AddAsync(newResponse);

            await _responseRepos.SaveChangesAsync();

            return _map.Map<ResponseDTO>(newResponse);
        }
  
    }
}