namespace U7Quizzes.DTOs.Session
{
    public class ParticipantDTO
    {
        public int ParticipantId { get; set; }
        public string? UserID { get; set; }
        public string DisplayName { get; set; }

        public string ConnectionId { get; set; }

    }
}