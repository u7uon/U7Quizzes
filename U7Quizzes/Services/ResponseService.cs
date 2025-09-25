using AutoMapper;
using U7Quizzes.DTOs.Session;
using U7Quizzes.IRepository;
using U7Quizzes.IServices;
using U7Quizzes.Models;

namespace U7Quizzes.Services
{
    public class ResponseService : IResponseService
    {
        private readonly IQuestionsRepository _quesRepos;
        private readonly IAnswerRepository _answerRepos;
        private readonly IResponseRepository _responseRepos;
        private readonly IMapper _map;

        public ResponseService(
            IQuestionsRepository quesRepos,
            IAnswerRepository answerRepos,
            IResponseRepository responseRepos,
            IMapper map)
        {
            _answerRepos = answerRepos;
            _quesRepos = quesRepos;
            _responseRepos = responseRepos;
            _map = map;
        }

        public async Task<ResponseDTO> SendResponse(ResponseSendDTO request)
        {
            if (await _responseRepos.IsResponseSubmitted(request.ParticipantId, request.QuestionID))
                throw new Exception("Already submitted response");

            var question = await _quesRepos.GetById(request.QuestionID);

            if (question == null)
                throw new NullReferenceException("Question not found");

            Response res = question.Type switch
            {
                QuestionType.SingleChoice or QuestionType.TrueFalse
                    => await SubmitSingleChoiceAnswer(question, request),

                QuestionType.MultipleChoice
                    => await SubmitMultiChoiceAnswer(question, request),

                QuestionType.ShortAnswer
                    => await SubmitShortAnswer(question, request),

                _ => throw new Exception("Cannot submit answer, please try later")
            };

            await _responseRepos.AddAsync(res);
            await _responseRepos.SaveChangesAsync();

            return _map.Map<ResponseDTO>(res);
        }

        private async Task<Response> SubmitSingleChoiceAnswer(Question ques, ResponseSendDTO request)
        {
            Response newResponse;

            if (request.AnswerIds != null && request.AnswerIds.Any())
            {
                var answerId = request.AnswerIds.First();
                var answer = await _answerRepos.GetAnswerById(answerId);

                if (answer == null || answer.Question.QuestionId != request.QuestionID)
                    throw new Exception("Answer or question not valid");

                newResponse = new Response
                {
                    ParticipantId = request.ParticipantId,
                    QuestionId = answer.Question.QuestionId,
                    ResponseAnswers = new List<ResponseAnswer> { new ResponseAnswer { AnswerId = answerId } },
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

            return newResponse;
        }

        private async Task<Response> SubmitMultiChoiceAnswer(Question ques, ResponseSendDTO request)
        {
            if (request.AnswerIds == null || !request.AnswerIds.Any())
            {
                return new Response
                {
                    ParticipantId = request.ParticipantId,
                    QuestionId = request.QuestionID,
                    IsCorrect = false,
                    Score = 0
                };
            }
            var selectedIds = request.AnswerIds ?? throw new NullReferenceException("Please choice least one "); 
            //var answers = await _answerRepos.GetAnswersByIds(request.AnswerIds);
            var correctAnswers = ques.Answers.Where(a => a.IsCorrect).Select(a => a.AnswerId).ToList();

            bool allCorrect = !correctAnswers.Except(request.AnswerIds).Any()
                              && !request.AnswerIds.Except(correctAnswers).Any();

            return new Response
            {
                ParticipantId = request.ParticipantId,
                QuestionId = ques.QuestionId,
                // Có thể lưu dưới dạng chuỗi danh sách Id (nếu model hỗ trợ nhiều đáp án thì cần bảng join)
                IsCorrect = allCorrect,
                Score = allCorrect ? ques.Points : 0 , 

                ResponseAnswers = selectedIds.Select(id => new ResponseAnswer
                {
                    AnswerId = id
                }).ToList()
            };
        }

        private async Task<Response> SubmitShortAnswer(Question ques, ResponseSendDTO request)
        {
            if (string.IsNullOrWhiteSpace(request.TextAnswer))
            {
                return new Response
                {
                    ParticipantId = request.ParticipantId,
                    QuestionId = request.QuestionID,
                    IsCorrect = false,
                    Score = 0,
                    TextResponse = ""
                };
            }

            
            var correctAnswer = ques.Answers.FirstOrDefault(a => a.IsCorrect);
            bool isCorrect = correctAnswer != null &&
                             string.Equals(correctAnswer.Content?.Trim(),
                                           request.TextAnswer?.Trim(),
                                           StringComparison.OrdinalIgnoreCase);

            return new Response
            {
                ParticipantId = request.ParticipantId,
                QuestionId = ques.QuestionId,
                IsCorrect = isCorrect,
                Score = isCorrect ? ques.Points : 0,
                TextResponse = request.TextAnswer
            };
        }
    }
}
