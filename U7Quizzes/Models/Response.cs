using System.ComponentModel.DataAnnotations;

namespace U7Quizzes.Models
{
    public class Response
    {
        [Key]
        public int ResponseId { get; set; }

        public int ParticipantId { get; set; }

        public int QuestionId { get; set; }

        public int? AnswerId { get; set; }

        public string TextResponse { get; set; }

        public bool IsCorrect { get; set; }

        public int Score { get; set; }

        public int ResponseTime { get; set; } // milliseconds

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }

        // Navigation properties
        public virtual Participant Participant { get; set; }
        public virtual Question Question { get; set; }
        public virtual Answer Answer { get; set; }
    }
}
