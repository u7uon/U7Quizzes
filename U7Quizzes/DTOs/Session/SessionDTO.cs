using U7Quizzes.Models; 
namespace U7Quizzes.DTOs.Session
{
    public class SessionDTO
    {
        public int QuizId { get; set; }
        public string AccessCode { get; set; }

        public string HostName { get; set;  }

        public SessionStatus Status { get; set; }



    }
}