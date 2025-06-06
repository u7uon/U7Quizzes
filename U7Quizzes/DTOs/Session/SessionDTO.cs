using System.Text.Json.Serialization;
using U7Quizzes.Models; 
namespace U7Quizzes.DTOs.Session
{
    public class SessionDTO
    {
        public int QuizId { get; set; }

        public int SessionID { get; set; }
        public string AccessCode { get; set; }

        public string HostName { get; set;  }

        public string SessionStatus => Status.ToString();  

        [JsonIgnore]
        public SessionStatus Status { get; set; }



    }
}