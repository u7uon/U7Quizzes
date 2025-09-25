using Microsoft.Identity.Client;
using U7Quizzes.Models;

namespace U7Quizzes.DTOs.Session
{
    public class ResponseSendDTO
    {
        public int ParticipantId { get; set; }
        public int QuestionID { get; set; } 

        public string? TextAnswer { get; set; }
        
        public int[]? AnswerIds { get; set; }
    }
}