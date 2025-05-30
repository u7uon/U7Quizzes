
using U7Quizzes.DTOs.Session; 


namespace U7Quizzes.IServices
{
    public interface IResponseService
    {
        Task<ResponseDTO> SendResponse(ResponseSendDTO request );
    }
}