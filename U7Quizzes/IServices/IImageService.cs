using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Mvc;
using U7Quizzes.DTOs;

namespace U7Quizzes.IServices
{
    public interface IImageService
    {
        CloudKeyDTO GenerateUploadKey(); 
        Task<string> UploadsAsync(IFormFile image);

        Task DeleteAsync(string fileName);

        Task<FileStreamResult> GetAsync(string filename); 
    }
}
