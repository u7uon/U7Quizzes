using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Mvc;

namespace U7Quizzes.IServices
{
    public interface IImageService
    {
        Task<string> UploadsAsync(IFormFile image);

        Task DeleteAsync(string fileName);

        Task<FileStreamResult> GetAsync(string filename); 
    }
}
