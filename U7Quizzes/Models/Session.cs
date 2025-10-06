using System.ComponentModel.DataAnnotations;

namespace U7Quizzes.Models
{
    public class Session
    {
        [Key]
        public int SessionId { get; set; }

        public int QuizId { get; set; }

        public string HostId { get; set; }

        [Required]
        [MaxLength(10)]
        public string AccessCode { get; set; }


        [Required]
        public SessionStatus Status { get; set; } = SessionStatus.Waiting;

        public DateTime? StartTime { get; set; }

        public DateTime? EndTime { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public string? ConnectionId { get; set; }

        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }

        // Navigation properties
        public virtual Quiz Quiz { get; set; }
        public virtual User Host { get; set; }
        public virtual ICollection<Participant> Participants { get; set; }
        public virtual SessionReport Report { get; set; }
    }

    public enum SessionStatus
    {
        Waiting,
        Active,
        Paused,
        Finished,
        Cancelled
    }
}
