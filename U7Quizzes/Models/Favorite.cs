using System.ComponentModel.DataAnnotations;

namespace U7Quizzes.Models
{
    public class Favorite
    {
        [Key]
        public int FavoriteId { get; set; }

        public string UserId { get; set; }

        public int QuizId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual User User { get; set; }
        public virtual Quiz Quiz { get; set; }
    }
}
