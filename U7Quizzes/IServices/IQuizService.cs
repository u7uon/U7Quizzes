using Azure;
using U7Quizzes.DTOs;
using U7Quizzes.DTOs.Quiz;
using U7Quizzes.DTOs.Response;
using U7Quizzes.DTOs.Share;

namespace U7Quizzes.IServices
{
    public interface IQuizService
    {

        Task<List<QuizDTO>> GetAllAsync();
        Task<QuizDTO?> GetByIdAsync(int id);
        Task<ServiceResponse<QuizDTO>> CreateAsync(QuizCreateDTO dto, string creatorId , IFormFile image);
        Task<bool> UpdateAsync(QuizUpdateDTO dto);
        Task<bool> DeleteAsync(int id);


        Task<PagedResult<QuizSearch>> GetByTagName(QuizFilter filter); 
    }
}
