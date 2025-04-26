using System.ComponentModel.DataAnnotations;

namespace U7Quizzes.Models
{
    public class RefreshToken
    {
        [Key]
        public int Id { get; set; }
        public string Token { get; set; } = null!;
        public DateTime ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // FK
        public string UserId { get; set; } = null!;
        public User User { get; set; } = null!;
    }
}
