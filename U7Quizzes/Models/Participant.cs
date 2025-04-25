using System.ComponentModel.DataAnnotations;

namespace U7Quizzes.Models
{
    public class Participant
    {
        [Key]
        public int ParticipantId { get; set; }

        public int SessionId { get; set; }

        public string? UserId { get; set; }

        [Required]
        [MaxLength(50)]
        public string Nickname { get; set; }

        public int Score { get; set; } = 0;

        public int? Rank { get; set; }

        public DateTime JoinTime { get; set; } = DateTime.UtcNow;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;


        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }

        // Navigation properties
        public virtual Session Session { get; set; }
        public virtual User User { get; set; }
        public virtual ICollection<Response> Responses { get; set; }
    }
}

