using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Mvc;
using U7Quizzes.IServices;

namespace U7Quizzes.Services
{
    public class ImageService : IImageService
    {
        private readonly Cloudinary _cloudinary; 
        private readonly IWebHostEnvironment _env;
        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };

        public ImageService(IWebHostEnvironment env , Cloudinary cloudinary )
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
                throw new ArgumentException("Ảnh không hợp lệ");


            var extension = Path.GetExtension(image.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(extension))
                throw new InvalidOperationException("Chỉ cho phép các định dạng ảnh: .jpg, .jpeg, .png, .webp");    

            // Tạo tên file duy nhất
            var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(image.FileName)}";

            var uploadParams = new ImageUploadParams()
            {
                File = new FileDescription(image.FileName),
                UseFilename = true,
                UniqueFilename = false,
                Overwrite = true
            };

            var uploadResult = _cloudinary.Upload(uploadParams);

            var imagePath = Path.Combine(_env.WebRootPath, "images", uniqueFileName);

            // Tạo thư mục nếu chưa có
            var directory = Path.GetDirectoryName(imagePath);
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory!);

            // Lưu file
            using (var stream = new FileStream(imagePath, FileMode.Create))
            {
                await image.CopyToAsync(stream);
            }

            return $"{uniqueFileName}";
        }
    }
}
