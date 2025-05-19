namespace U7Quizzes.DTOs.Quiz
{
    public class QuizSearch
    {
        public int QuizId { get; set;  }
        public string Title { get; set; }

        public string ImageUrl { get; set; }

        public int TotalAttempts { get; set; }           
        public int QuestionCount { get; set; } 
    }
}
