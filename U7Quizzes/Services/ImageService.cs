using System.Threading.Tasks;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Mvc;
using U7Quizzes.DTOs;
using U7Quizzes.IServices;

namespace U7Quizzes.Services
{
    public class ImageService : IImageService
    {
        private readonly Cloudinary _cloudinary;
        private readonly IWebHostEnvironment _env;
        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };

        public ImageService(IWebHostEnvironment env, Cloudinary cloudinary)
        {
            _env = env;
            _cloudinary = cloudinary;
        }


        public async Task DeleteAsync(string fileName)
        {
            var fullPath = Path.Combine(_env.WebRootPath, "images", fileName);

            if (File.Exists(fullPath))
            {
                await Task.Run(() => File.Delete(fullPath));
            }
        }

        public async Task<FileStreamResult> GetAsync(string fileName)
        {
            var fullPath = Path.Combine(_env.WebRootPath, "images", fileName);

            if (!File.Exists(fullPath))
                throw new FileNotFoundException("Image not found");

            var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read);
            var contentType = GetContentType(fileName);

            return new FileStreamResult(stream, contentType);
        }

        private string GetContentType(string fileName)
        {
            var ext = Path.GetExtension(fileName).ToLowerInvariant();
            return ext switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".webp" => "image/webp",
                _ => "application/octet-stream"
            };
        }

        public async Task<string> UploadsAsync(IFormFile image)
        {
            if (image == null || image.Length == 0)
                throw new ArgumentException("Image is null ");


            var extension = Path.GetExtension(image.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(extension))
                throw new InvalidOperationException("Only allow : .jpg, .jpeg, .png, .webp");

            
            await using var stream = image.OpenReadStream();

            var uploadParams = new ImageUploadParams()
            {
                File = new FileDescription(image.FileName, stream),
                UseFilename = true,
                UniqueFilename = true,
                Overwrite = false
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

            if (uploadResult.StatusCode != System.Net.HttpStatusCode.OK)
                throw new Exception(uploadResult.Error?.Message);

            Console.WriteLine("Upload success");
            return uploadResult.DisplayName;
        }



        public  CloudKeyDTO GenerateUploadKey()
        {
            var timeSpan = TimeSpan.FromMinutes(3).ToString();

            var para = new Dictionary<string, object>
            {
                {"timespan",timeSpan},
                {"folder","u7quizzes_image"},
                { "allowed_formats", "jpg,png,webp" }, 
                { "resource_type", "image" }
            };

            var signature =  _cloudinary.Api.SignParameters(para);


            return new CloudKeyDTO
            {
                Exp = timeSpan,
                Signature = signature
            }; 

        }
    }
}
