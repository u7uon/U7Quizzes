using U7Quizzes.Models;

namespace U7Quizzes.DTOs.Quiz
{
    public class QuestionCreateDTO
    {
        public string Content { get; set; }
        public string Explanation { get; set; }
        public QuestionType Type { get; set; }
        public List<AnswerCreateDTO> Answers { get; set; }
    }
}
