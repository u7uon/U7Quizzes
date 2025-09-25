namespace U7Quizzes.Models
{
    public class ResponseAnswer
    {
        public int ResponseId { get; set; }
        public Response Response { get; set; }

        public int AnswerId { get; set; }
        public Answer Answer { get; set; }
    }
}
