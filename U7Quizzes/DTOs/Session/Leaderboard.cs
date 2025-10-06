using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using Microsoft.Identity.Client;

namespace U7Quizzes.DTOs.Session
{
    public class Leaderboard
    {
        public int SessionId { get; set; }

        public List<Participant_Leaderboard> Participants { get; set; }

    }


    public class Participant_Leaderboard
    {

        public ParticipantDTO Participant { get; set; }

        public int Rank { get; set; }

        public int Score { get; set; }

        public int WrongAwnserCount { get; set; }

        public int CorrectAnsCount { get; set; }

    }
}
