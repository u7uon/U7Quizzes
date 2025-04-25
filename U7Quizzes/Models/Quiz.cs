using System.ComponentModel.DataAnnotations;

namespace U7Quizzes.Models
{
    public class Quiz
    {
        [Key]
        public int QuizId { get; set; }

        public string CreatorId { get; set; }

        [Required]
        [MaxLength(255)]
        public string Title { get; set; }

        public string Description { get; set; }

        public int? TimeLimit { get; set; }

        public bool IsPublic { get; set; } = true;

        [MaxLength(255)]
        public string CoverImage { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;


        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }



        // Navigation properties
        public virtual User Creator { get; set; }
        public virtual ICollection<Question> Questions { get; set; }
        public virtual ICollection<Session> Sessions { get; set; }
        public virtual ICollection<QuizTag> QuizTags { get; set; }
        public virtual ICollection<Favorite> Favorites { get; set; }
        public virtual ICollection<QuizCategory> QuizCategories { get; set; }


        //public virtual ICollection<QuizAccess> QuizAccesses { get; set; }
    }
}
