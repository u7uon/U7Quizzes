using U7Quizzes.Models;

namespace U7Quizzes.DTOs.Session
{
    public class ResponseDTO
    {
        public int ResponseId { get; set; }

        public int ParticipantId { get; set; }

        public int QuestionId { get; set; }

        //public int[] AnswerIds { get; set; }

        public bool IsCorrect { get; set; }

        public int Score { get; set; }

        public int ResponseTime { get; set; } // milliseconds

    }
}