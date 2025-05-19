
using U7Quizzes.DTOs.Response; 


namespace U7Quizzes.DTOs.Quiz
{
    public class QuizFilter : FilterBase
    {
        public List<int>? Tags { get; set; }

        public List<int>? Category { get; set; }
        
    }
}
