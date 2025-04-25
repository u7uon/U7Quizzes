using System.ComponentModel.DataAnnotations;

namespace U7Quizzes.Models
{
    public class QuizCategory
    {
        [Key]
        public int QuizCategoryId { get; set; }

        public int QuizId { get; set; }

        public int CategoryId { get; set; }

        // Navigation properties
        public virtual Quiz Quiz { get; set; }
        public virtual Category Category { get; set; }
    }
}
