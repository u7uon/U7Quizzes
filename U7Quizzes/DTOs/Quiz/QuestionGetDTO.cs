using System.Text.Json.Serialization;
using U7Quizzes.Models;

namespace U7Quizzes.DTOs.Quiz
{
    public class QuestionGetDTO
    {
        public int QuestionId { get; set; }
        public string Content { get; set; }
        public string? Explanation { get; set; }

        public string? ImageUrl { get; set; }

        public int TimeLimit { get; set; }

        public QuestionType Type { get; set; }

        public string QuestionType => Type.ToString(); 

        public List<AnswerGetDTO> Answers { get; set; }
    }
}
