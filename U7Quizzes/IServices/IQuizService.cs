using U7Quizzes.DTOs.Quiz;
using U7Quizzes.DTOs.Share;

namespace U7Quizzes.IServices
{
    public interface IQuizService
    {
        Task<List<QuizDTO>> GetAllAsync();
        Task<QuizDTO?> GetByIdAsync(int id);
        Task<ServiceResponse<QuizDTO>> CreateAsync(QuizCreateDTO dto, string creatorId);
        Task<bool> UpdateAsync(QuizUpdateDTO dto);
        Task<bool> DeleteAsync(int id);


        Task<QuizFilter> GetByTagName(QuizFilter filter); 
    }
}
