using System.ComponentModel.DataAnnotations;

namespace U7Quizzes.Models
{
    public class QuizTag
    {
        [Key]
        public int QuizTagId { get; set; }

        public int QuizId { get; set; }

        public int TagId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual Quiz Quiz { get; set; }
        public virtual Tag Tag { get; set; }
    }
}
