using U7Quizzes.Models;

namespace U7Quizzes.DTOs.Quiz
{
    public class QuestionDTO
    {
        public int QuestionId { get; set; }
        public string Content { get; set; }
        public string? Explanation { get; set; }
        public QuestionType Type { get; set; }

        public List<AnswerDTO> Answers { get; set; }
    }
}
