using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace U7Quizzes.Models
{
    public class SessionReport
    {
        [Key]
        public int ReportId { get; set; }

        public int SessionId { get; set; }

        public float AverageScore { get; set; }

        public float CompletionRate { get; set; }

        [Column(TypeName = "nvarchar(max)")]
        public string QuestionStatistics { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual Session Session { get; set; }
    }
}
