using Microsoft.AspNetCore.SignalR;

namespace U7Quizzes.SingalIR
{
    public class QuizSessionHub : Hub
    {


     
        public async Task StartSession(int sessionId)
        {
            // 1. Cập nhật trạng thái session -> Active
            // 2. Lấy câu hỏi đầu tiên
            // 3. Gửi xuống tất cả người chơi
        }
        
    }
}
