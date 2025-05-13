using Microsoft.AspNetCore.Http.Metadata;

namespace U7Quizzes.IServices
{
    public interface IImageService
    {
        Task<string> UploadsAsync(IFormFile image);

        Task DeleteAsync(string fileName);

        Task GetAsync(string filename); 
    }
}
