using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace U7Quizzes.Models
{
    public class Question
    {
        [Key]
        public int QuestionId { get; set; }

        public int QuizId { get; set; }

        [Required]
        public string Content { get; set; }

        [MaxLength(255)]
        public string? ImageUrl { get; set; }

        [Required]
        public QuestionType Type { get; set; } 

        public int Points { get; set; } = 10;

        public int? TimeLimit { get; set; }

        public int Position { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }

        // Navigation properties
        public virtual Quiz Quiz { get; set; }
        public virtual ICollection<Answer> Answers { get; set; }
        public virtual ICollection<Response> Responses { get; set; }

    }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum QuestionType
    {
        TrueFalse = 0,
        SingleChoice = 1,
        MultipleChoice = 2,
        ShortAnswer = 3
    }
}
