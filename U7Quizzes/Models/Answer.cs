using System.ComponentModel.DataAnnotations;

namespace U7Quizzes.Models
{
    public class Answer
    {
        [Key]
        public int AnswerId { get; set; }

        public int QuestionId { get; set; }

        [Required]
        public string Content { get; set; }

        [MaxLength(255)]
        public string? ImageUrl { get; set; }

        public bool IsCorrect { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }

        // Navigation properties
        public virtual Question Question { get; set; }
        //public virtual ICollection<Response> Responses { get; set; }
    }
}
