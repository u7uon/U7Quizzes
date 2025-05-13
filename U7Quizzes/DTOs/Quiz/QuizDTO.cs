namespace U7Quizzes.DTOs.Quiz
{
    public class QuizDTO
    {
        public int QuizId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int? TimeLimit { get; set; }
        public bool IsPublic { get; set; }
        public string? CoverImage { get; set; }
        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public List<QuestionDTO> Questions { get; set; }
        public List<string> Categories { get; set; }
        public List<string> Tags { get; set; }
    }
}
