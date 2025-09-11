     using Microsoft.AspNetCore.Identity;

namespace U7Quizzes.Models
{
    public class User : IdentityUser
    {
        public string DisplayName { get; set; }
        public virtual ICollection<Quiz> Quizzes { get; set; }
        public virtual ICollection<Session> HostedSessions { get; set; }
        public virtual ICollection<Participant> Participations { get; set; }
        public virtual ICollection<Favorite> Favorites { get; set; }

    }
}
