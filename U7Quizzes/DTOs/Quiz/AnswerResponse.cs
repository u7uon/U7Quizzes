namespace U7Quizzes.DTOs.Quiz
{
    public class AnswerResponse
    {
        public int AnswerId { get; set; }

        public int QuestionId { get; set; }

        public bool IsCorrect { get; set; }

        public QuestionResponse Question { get; set; }

    }


    public class QuestionResponse
    {
        public int QuestionId { get; set; }
        
        public int Points { get; set;  }
    }
}
