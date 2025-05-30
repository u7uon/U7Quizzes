namespace U7Quizzes.DTOs.Quiz
{
    public class QuizCreateDTO
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public int? TimeLimit { get; set; }
        public bool IsPublic { get; set; }
        public string? CoverImage { get; set; }
        public List<int> CategoryIds { get; set; }
        public List<int> TagIds { get; set; }

        public List<QuestionCreateDTO> Questions { get; set; }
    }
}
